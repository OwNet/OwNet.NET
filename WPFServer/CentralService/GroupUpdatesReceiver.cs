using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPFServer.DatabaseContext;
using System.ServiceModel.Web;

namespace WPFServer.CentralService
{
    class GroupUpdatesReceiver
    {
        internal static void Process(LocalServerCentralService.GroupReport[] groupReport, LocalServerCentralService.GroupRecommendationReport[] groupRecommendationReport)
        {
            try
            {
                using (MyDBContext con = new MyDBContext())
                {
                    foreach (var gR in groupReport)
                    {
                        IQueryable<Group> groups = con.Fetch<Group>(g => g.Id == gR.Id);
                        if (groups.Any())
                        {
                            var g = groups.First();
                            g.Name = gR.Name;
                            g.Description = gR.Description;
                        }
                        else
                        {
                            Group group = new Group();
                            group.Name = gR.Name;
                            group.Description = gR.Description;
                            group.Id = gR.Id;
                            group.Location = true;
                            group = con.FetchOrCreate<Group>(group);
                        };
                    }
                    con.SaveChanges();

                    var user = con.FetchOrCreate<User>(new User()
                    {
                        Firstname = "Global",
                        Surname = "OwNet",
                        Username = "ownetglobal",
                        Password = Helpers.Common.RandomString(12, true),
                        Gender = UserGender.Male,
                        IsTeacher = false
                    });

                    foreach (var recommendation in groupRecommendationReport)
                    {
                        var group = con.Fetch<Group>(g => g.Id == recommendation.GroupId).FirstOrDefault();
                        if (group != null)
                        {
                            Page page = con.FetchOrCreate<Page>(new Page() { AbsoluteURI = recommendation.AbsoluteUri, Title = recommendation.Title }, true);
                            
                            con.Create<GroupRecommendation>(new GroupRecommendation()
                            {
                                DateTime = DateTime.Now,
                                Page = page,
                                Title = recommendation.Title,
                                Description = recommendation.Description,
                                User = user,
                                Group = group
                            });

                            con.SaveChanges();
                        }
                    }
                }
            }
            catch (MyDBContextException e)
            {
                throw new WebFaultException<ServiceEntities.ErrorHandler>(new ServiceEntities.ErrorHandler { cause = e.Message, inner = e.InnerException != null ? e.InnerException.Message : "", errorCode = 500 }, System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
