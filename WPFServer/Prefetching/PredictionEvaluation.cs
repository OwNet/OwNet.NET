using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using WPFServer.DatabaseContext;

namespace WPFServer.Prefetching
{
    class PredictionEvaluation
    {

        public static ServiceEntities.PrefetchLinks Predict(string uri)
        {
            return PredictionEvaluation.PredictReferrerGraph(uri, null);
        }

        public static ServiceEntities.PrefetchLinks Predict(string uri, int? userid)
        {

            if (Properties.Settings.Default.CollaborativeFiltering)
            {
                return PredictionEvaluation.PredictCollaborativeFiltering(uri, userid);
            }
            else
            {
                return PredictionEvaluation.PredictReferrerGraph(uri, userid);
            }
        }

        private static ServiceEntities.PrefetchLinks PredictCollaborativeFiltering(string uri, int? userid = null)
        {

            ServiceEntities.PrefetchLinks ret = new ServiceEntities.PrefetchLinks();

            ret.From.AbsoluteURI = uri;

            using (MyDBContext con = new MyDBContext())
            {
                try
                {
                    IQueryable<Page> pages = con.Fetch<Page>(new Page() { AbsoluteURI = uri });
                    if (pages.Any())
                    {
                        Page page = pages.First();
                        IQueryable<User> users = con.Fetch<User>(u => u.Id == userid);
                        ret.From.Title = page.Title;
                        double sum1 = 0;
                        double sum2 = 0;


                        if (users.Any())
                        {   // personalized prediction
                            User user = users.First();

                            // hrany veduce zo stranky
                            IEnumerable<Edge> edges = page.EdgesFrom
                                .Where(e => !e.Traverses.Any(t => t.UserId == userid));// || t.EstimatedFrequency == null));

                            // pouzivatelia, ktori presli hranou
                            IEnumerable<User> edgeUsers = edges
                                .SelectMany(e => e.Traverses)
                                .Where(t => t.TimeStamp != null)    // skutocne presli hranou
                                .Select(t => t.User);

                            // pouzivatelia, ktori navstivili stranky v cieli hrany
                            IEnumerable<User> visitorUsers = edges
                                .SelectMany(e => e.PageTo.Visitors)
                                .Where(v => v.UserId != userid && v.VisitedAt != null && v.RatedAt != null && v.Rating != null)
                                .Select(v => v.User);

                            // zjednotenie pouzivatelov
                            IEnumerable<User> weightUsers = edgeUsers
                                .Concat(visitorUsers)
                                .GroupBy(u => u.Id)
                                .Select(y => y.First());

                            IQueryable<UserSimilarity> sims;
                            UserSimilarity sim;
                            Dictionary<int, double> weights = new Dictionary<int, double>(weightUsers.Count());
                            double similarity = 0.0;


                            foreach (User us in weightUsers)
                            {
                                // ziskaj podobnost pouzivatelov
                                sims = con.Fetch<UserSimilarity>(new UserSimilarity() { UserLeft = user, UserRight = us });

                                if (sims.Any())
                                {   // ak existuje
                                    sim = sims.First();

                                    DateTime border = sim.EvaluatedAt.AddHours(1);
                                    int changes = 0;
                                    changes += us.Visits.Count(v => v.RatedAt != null && v.RatedAt > border);
                                    changes += us.GroupRecommendations.Count(r => r.DateTime > border);

                                    // ak vykonal zmeny (hodnotil alebo odporucil)
                                    if (changes > 2)
                                    {   // prepocitaj podobnost a uloz zmenu
                                        sim.Value = ComputeUserSimilarity(user, us);
                                        con.SaveChanges();
                                    }
                                }
                                else
                                {   // inak vypocitaj podobnost
                                    similarity = ComputeUserSimilarity(user, us);

                                    // uloz podobnost
                                    sim = con.FetchOrCreate<UserSimilarity>(new UserSimilarity() { UserLeft = user, UserRight = us, Value = similarity }, true);
                                    con.SaveChanges();
                                }
                                weights.Add(us.Id, sim.Value);  // prirad do slovnika podobnosti
                            }

                            // pre kazdu hranu veducu zo stranky
                            foreach (Edge edge in edges)
                            {
                                if (!edge.PageTo.Visitors.Any(v => v.UserId == user.Id && v.Rating != null))
                                {   // ak pouzivatel stranku na konci hrany este nenavstivil
                                    sum1 = 0;
                                    sum2 = 0;
                                    // vypocitaj predpokladane hodnotenie
                                    List<UserVisitsPage> visits = edge.PageTo.Visitors.Where(v => v.Rating != null && v.RatedAt != null).ToList();
                                    foreach (UserVisitsPage visit in visits)
                                    {
                                        if (weights.ContainsKey(visit.UserId))
                                        {
                                            sum1 += weights[visit.UserId] * (visit.Rating ?? 0);
                                            sum2 += weights[visit.UserId];
                                        }
                                    }
                                    double estRating = sum1 / sum2;
                                    // uloz predpokladane hodnotenie
                                    con.FetchOrCreate<UserVisitsPage>(
                                        new UserVisitsPage()
                                        {
                                            User = user,
                                            Page = edge.PageTo,
                                            EstimatedRating = double.IsNaN(estRating) ? 0.0 : estRating,
                                            Rating = null,
                                            RatedAt = null,
                                            VisitedAt = null
                                        }, true);
                                }

                                sum1 = 0;
                                sum2 = 0;
                                // vypocitaj odhad frekvencie prechodov cez hranu
                                foreach (UserTraversesEdge traverse in edge.Traverses)
                                {
                                    if (weights.ContainsKey(traverse.UserId))
                                    {
                                        sum1 += weights[traverse.UserId] * (traverse.Frequency);
                                        sum2 += weights[traverse.UserId];
                                    }
                                }
                                double estFrequency = sum1 / sum2;
                                // uloz
                                con.FetchOrCreate<UserTraversesEdge>(
                                    new UserTraversesEdge()
                                    {
                                        Edge = edge,
                                        User = user,
                                        Frequency = 0,
                                        EstimatedFrequency = double.IsNaN(estFrequency) ? 0.0 : estFrequency,
                                        TimeStamp = null
                                    }, true);
                            }

                            IEnumerable<UserVisitsPage> currentVisits = page.Visitors.Where(v => v.UserId == user.Id);
                            int currentCount = 0;
                            if (currentVisits.Any())
                            {
                                currentCount = currentVisits.First().Count;
                            }
                            // vypocitaj pravdepodobnost navstivenia stranky
                            var predictions = user
                               .Traverses
                               .Where(t => t.Edge.PageFromId == page.Id)
                               .Join(
                                  user.Visits,
                                  t => t.Edge.PageToId,
                                  v => v.PageId,
                                  (t, v) => new
                                  {
                                      Page = t.Edge.PageTo,
                                      Value =
                                        (t.Frequency == 0 ? ((double)(t.EstimatedFrequency ?? 0.0) / (currentCount + (t.EstimatedFrequency ?? 0.0))) : (((double)t.Frequency) / currentCount)) * 0.4
                                        + (v.Rating == null ? (double)(v.EstimatedRating ?? 0.0) : (double)(v.Rating ?? 0)) / 5 * 0.6
                                  })
                               .OrderByDescending(p => p.Value) // zorad zostupne
                               .ToList();

                            // pridaj do vystupu
                            foreach (var pred in predictions)
                            {
                                ret.Links.Add(new ServiceEntities.PageObjectWithRating()
                                {
                                    AbsoluteURI = pred.Page.AbsoluteURI,
                                    Title = pred.Page.Title,
                                    AvgRating = pred.Value,
                                    Id = pred.Page.Id
                                });
                                ret.Count += 1;
                                con.FetchOrCreate<Prediction>(new Prediction() { User = user, Page = pred.Page });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            return ret;
        }
        private static ServiceEntities.PrefetchLinks PredictReferrerGraph(string uri, int? userid = null)
        {
            ServiceEntities.PrefetchLinks ret = new ServiceEntities.PrefetchLinks();
            try
            {
                ret.From.AbsoluteURI = uri;
                User user = null;

                using (MyDBContext con = new MyDBContext())
                {
                    try
                    {
                        IQueryable<Page> pages = con.Fetch<Page>(new Page() { AbsoluteURI = uri });

                        if (userid != null)
                        {
                            int uid = (int)userid;
                            IQueryable<User> users = con.Fetch<User>(u => u.Id == uid);
                            if (users.Any())
                            {
                                user = users.First();
                            }
                        }

                        if (pages.Any())
                        {
                            Page page = pages.First();
                            ret.From.Title = page.Title;

                            List<Page> predictions = null;

                            if (user == null)
                            {
                                predictions = page.EdgesFrom
                                    .Select(e => new
                                    {
                                        Page = e.PageTo,
                                        Visits = e.PageTo.Visitors.Sum(v => v.Count),
                                        Traverses = e.Traverses.Sum(t => t.Frequency),
                                        Rating = e.PageTo.Visitors.Average(v => (int?)v.Rating),
                                        //                                        Visit = user.Visits.Any(v => v.PageId == e.PageToId) ? user.Visits.First(v => v.PageId == e.PageToId) : null,
                                    })
                                    .Select(e => new
                                    {
                                        e,
                                        Value = (((double)e.Traverses) / e.Visits) * 0.4 + (((double)(e.Rating ?? 0.0)) / 5) * 0.6
                                    })
                                    .OrderByDescending(p => p.Value)
                                    .Select(e => e.e.Page)
                                    .ToList();
                            }
                            else
                            {
                                predictions = page.EdgesFrom
                                    .Select(e => new
                                    {
                                        Page = e.PageTo,
                                        Visits = e.PageTo.Visitors.Sum(v => v.Count),
                                        Traverses = e.Traverses.Sum(t => t.Frequency),
                                        Rating = e.PageTo.Visitors.Average(v => (int?)v.Rating) ?? 0.0,
                                        Visit = user.Visits.Any(v => v.PageId == e.PageToId) ? user.Visits.First(v => v.PageId == e.PageToId) : null,
                                    })
                                    .Select(e => new
                                    {
                                        e,
                                        Value = (((double)e.Traverses) / e.Visits) * 0.4 + (((double)(e.Visit != null && e.Visit.Rating != null ? (double) e.Visit.Rating : e.Rating)) / 5) * 0.6
                                    })
                                    .OrderByDescending(p => p.Value)
                                    .Select(e => e.e.Page)
                                    .ToList();
                            }
                            //var predictions = page
                            //    .EdgesFrom
                            //    .Select(e => new
                            //    {
                            //        Page = e.PageTo,
                            //        SumFrequency = e.Traverses.Sum(t => t.Frequency),
                            //        AvgRating = e.PageTo.Visitors.Average(v => (int?)v.Rating)
                            //    })
                            //    .OrderByDescending(a => a.AvgRating)
                            //    .ThenByDescending(a => a.SumFrequency)
                            //    .Take(maxcount)
                            //    .ToList();


                            foreach (Page pred in predictions)
                            {
                                ret.Links.Add(new ServiceEntities.PageObjectWithRating()
                                {
                                    AbsoluteURI = pred.AbsoluteURI,
                                    Title = pred.Title,
                                    Id = pred.Id,
                                });
                                ret.Count += 1;
                                con.FetchOrCreate<Prediction>(new Prediction() { User = user, Page = pred });
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (MyDBContextException e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
            return ret;
        }


        private static double ComputeUserSimilarity(User user, User us)
        {
            double sum1;
            double sum2;
            double sum3;

            Dictionary<int, int> ratings = user.Visits.Where(v => v.RatedAt != null).ToDictionary(v => v.PageId, v => v.Rating ?? 0);
            double avgrating = (ratings.Values.Any()) ? ratings.Values.Average() : 0.0;

            Dictionary<int, int> usratings = us.Visits.Where(v => ratings.Keys.ToList().Contains(v.PageId)).ToDictionary(v => v.PageId, v => v.Rating ?? 0);
            double usavgrating = us.Visits.Where(v => v.RatedAt != null).Average(v => v.Rating) ?? 0.0;


            sum1 = 0;
            sum2 = 0;
            sum3 = 0;
            foreach (int key in ratings.Keys)
            {
                if (usratings.ContainsKey(key))
                {
                    sum1 += (ratings[key] - avgrating) * (usratings[key] - usavgrating);
                    sum2 += (ratings[key] - avgrating) * (ratings[key] - avgrating);
                    sum3 += (usratings[key] - usavgrating) * (usratings[key] - usavgrating);
                }
            }

            return (sum2 > 0.0 && sum3 > 0.0) ? sum1 / Math.Sqrt(sum2 * sum3) : 0.0;
        }
    }
}

