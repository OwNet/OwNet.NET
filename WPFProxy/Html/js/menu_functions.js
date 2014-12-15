$(document).ready(function () {
    $.ajax
        ({
            type: "GET",
            // tu nastavime spravnu hlasovaciu url
            url: "http://my.ownet/recommend/not_displayed",
            dataType: 'json',
            success: function (data) {
                var count = 0;
                $.each(data, function (key, val) {
                    switch (key) {
                        case "count": count = (val); break;

                    }
                    if (count != 0) {
                        if (count > 99)
                            count = "99+";
                        $('<span style=\" position: relative; top: -7px; margin-left: -6px;line-height: 10px; font-size: 12px;\">[' + count + ']</span>').insertAfter("#recs_menu");
                    }

                });
                setRecommendationElements(set, user, title, desc, group);
                owNetIframeGLOBAL.recommend.desc = desc;
                owNetIframeGLOBAL.recommend.group = group;
                owNetIframeGLOBAL.recommend.title = title;
                if (set != 0) {
                    showRecommendationAsText(edit == 0 ? false : true);
                }
                else showRecommendationAsForm();
            }

        });
});
