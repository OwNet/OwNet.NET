/* Otvorenie noveho okna */
function NewWindow(weblink) {
    return window.open(weblink.href, "_blank");
}

function getEncodedParentPageUri() {
    var elem = document.getElementById("parent-uri");
    if (elem && elem.value.match(/\S/g) !== null) {
        return elem.value;
    }
    var str = document.location.href;
    return str.replace(/^.*parent=/, "");
}

owNetIframeGLOBAL = {
    rating: null,
    recommend: {
        userGroup: null,
        title: null,
        desc: null,
        group: null,
        isNull: function () { if (this.title === null || this.desc === null || this.group === null) return true; return false; }
    },
    tags: null
};

function showLoader(rootId) {
    var root = null;
    if (typeof (rootId) === "string")
        root = document.getElementById(rootId);
    else
        root = rootId;
    if (root === null) return null;
    var loaderclass = "ajax_loader_small";
    var childs = root.childNodes;
    for (var i = 0; i < childs.length; ++i) {
        if (childs[i].className === loaderclass) {
            return childs[i];
        }
    }
    var loader = document.createElement("div");
    loader.className = loaderclass;
    root.appendChild(loader);
    return loader;
}

function hideLoader(rootId, loaderElem) {
    if (loaderElem == null) return;
    var root = null;
    if (typeof (rootId) === "string")
        root = document.getElementById(rootId);
    else
        root = rootId;
    if (root === null) return;
    var childs = root.childNodes;
    for (var i = 0; i < childs.length; ++i) {
        if (childs[i] === loaderElem) {
            root.removeChild(loaderElem);
        }
    }
}

function setExternalLink(elem) {
    elem.setAttribute("href", decodeURIComponent(getEncodedParentPageUri()));
    var inner = decodeURIComponent(getEncodedParentPageUri());
    if (inner.length > 70) inner = inner.substr(0, 70) + " &hellip;";
    elem.innerHTML = inner;
}

function setRating(stars) {
    var param = 'rate';
    var num = stars * 20;
    if (stars !== null && owNetIframeGLOBAL.rating !== stars) {
        var loader = showLoader("tabstarrating");

        $.ajax({
            type: "POST",
            // tu nastavime spravnu hlasovaciu url
            url: "http://inject.ownet/rating/create",
            data: { page: decodeURIComponent(getEncodedParentPageUri()), rating: stars },
            dataType: 'json',
            success: function (data) {
                $.each(data, function (key, val) {
                    if (key === "status" && val === "OK") {
                        $("#current-rating-1").width("" + num + "%");
                        $("#itext").html("You've submitted rating for this web site.");
                        owNetIframeGLOBAL.rating = stars;
                    }
                    else {
                        showError("We are sorry, some error occurred.<br />Is your OwNet Client connected to OwNet Server?");
                    }
                });
            },
            complete: function () {
                hideLoader("tabstarrating", loader);
            }
        });
    }
}


function getRating() {
    var loader = showLoader("tabstarrating");

    $.ajax
({
    type: "GET",
    // tu nastavime spravnu hlasovaciu url
    url: "http://inject.ownet/rating/read?page=" + getEncodedParentPageUri(),
    dataType: 'json',
    success: function (data) {
        $.each(data, function (key, val) {
            if (key === "rating") {
                if (val !== "0") {
                    $("#itext").html("You've already rated this web site with <strong>" + val + " stars</strong>.");
                    owNetIframeGLOBAL.rating = val;
                }
            }
            else if (key === "avgrating") {
                $("#current-rating-1").width("" + val * 20 + "%");
            }
        });
    },
    complete: function () {
        hideLoader("tabstarrating", loader);
    }

});
}

function showRecommendationAsText(editbtn) {
    $("#rsubmit").hide();

    $("#rauthorlabel").show();
    $("#rauthortext").show();

    $("#rtitle").hide();
    $("#rtitletext").show();

    $("#rdesc").hide();
    $("#rdesctext").show();

    $("#rgroup").hide();
    $("#rgrouptext").show();

    $("#rselect-group").hide();
    $("#autocompleteGroups").hide();

    $("#rgrouphelp").hide();
    $("#rtitlehelp").hide();

    if (editbtn === true) {
        $("#redit").show();
    }
    else {
        $("#redit").hide();
    }
}

function showRecommendationAsForm() {
    $("#rsubmit").show();

    $("#rauthorlabel").hide();
    $("#rauthortext").hide();

    $("#rtitle").show();
    $("#rtitletext").hide();

    $("#rdesc").show();
    $("#rdesctext").hide();

    $("#rgroup").show();
    $("#rgrouptext").hide();
    $("#rgrouphelp").show();

    $('#autocompleteGroups').val('search groups...');

    
    $("#rtitlehelp").show();
    $("#redit").hide();
}

function setRecommendationElements(set, user, title, desc, group) {
    if (set != 0) {
        $("#rauthortext").html(user);

        $("#rtitle").val(title);
        $("#rtitletext").html(title);

        $("#rdesc").val(desc);
        $("#rdesctext").html(desc);

        $("#rgroup").val(group);
        $("#rgrouptext").html(group);
    }
    else {
    	$("#rauthortext").html("");

        $("#rtitle").val(title);
        $("#rtitletext").html(title);

        $("#rdesc").val(desc);
        $("#rdesctext").html(desc);

        $("#rgroup").val("--Choose--");
        $("#rgrouptext").html("");
    }
}

function getRecommendation() {
    var loader = showLoader("td-recomm-submit");
    $.ajax
({
    type: "GET",
    // tu nastavime spravnu hlasovaciu url
    url: "http://inject.ownet/recommend/read?page=" + getEncodedParentPageUri(),
    dataType: 'json',
    success: function (data) {
        var user = "";
        var desc = "";
        var group = "";
        var title = "";
        var set = 0;
        var edit = 0;
        $.each(data, function (key, val) {
            switch (key) {
                case "user": user = decodeURIComponent(val); break;
                case "desc": desc = decodeURIComponent(val);  break;
                case "group": group = decodeURIComponent(val); break;
                case "title": title = decodeURIComponent(val);  break;
                case "set": set = (val === "1") ? 1 : 0; break;
                case "edit": edit = (val === "1") ? 1 : 0; break;
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
    },
    complete: function () {
        hideLoader("td-recomm-submit", loader);
    }
});
}

function showError(text) {
    $.fancybox(
        "<h2>Error</h2><p>" + text + "</p>",
        {
            'autoDimensions': false,
            'width': 350,
            'height': 'auto',
            'transitionIn': 'none',
            'transitionOut': 'none',
            'showCloseButton': false,
            'hideOnContentClick': true
        }
    );
}

function setRecommendation() {

    var rtitle = $("#rtitle").val();
    var rdesc = $("#rdesc").val();
    
    /* Marek skupina*/
    if ($("#autocompleteGroups").val() != "" && $("#autocompleteGroups").val() != "search groups...")
        var ruserGroup = $("#autocompleteGroups").val();
    else
        var ruserGroup = $("#rselect-group").val();

    rtitle = (rtitle.length > 70) ? rtitle.substr(0, 70) : rtitle;
    rdesc = (rdesc.length > 300) ? rdesc.substr(0, 300) : rdesc;

    if (rdesc.match(/\S/g) === null) {
        showError("You have to specify description.");
        return;
    }
    if (rtitle.match(/\S/g) === null) {
        showError("You have to specify title.");
        return;
    }
    var loader = showLoader("td-recomm-submit");

    $.ajax
    ({
        type: "POST",
        url: "http://inject.ownet/recommend/create",
        data: { page: decodeURIComponent(getEncodedParentPageUri()), desc: rdesc, title: rtitle, userGroup: ruserGroup },
        success: function (data) {
            $.each(data, function (key, val) {
                if (key === "status" && val === "OK") {
                    //setRecommendationElements(1, "you", rtitle, rdesc, ruserGroup);
                    owNetIframeGLOBAL.recommend.title = rtitle;
                    owNetIframeGLOBAL.recommend.desc = rdesc;
                    owNetIframeGLOBAL.recommend.userGroup = ruserGroup;

                    //showRecommendationAsText(true);

                    $('#previousrecommendations_tbody').append('<tr><td>' + rtitle + '</td><td>' + ruserGroup + '</td></tr>');
                    $('#previousrecommendations').show();

                    $('#messages').html('<div class="message_success">Website <strong>successfully</strong> recommended!</div>');
                }
                else if (key === "status" && val === "UPDATED") {
                    //setRecommendationElements(1, "you", rtitle, rdesc, ruserGroup);
                    owNetIframeGLOBAL.recommend.title = rtitle;
                    owNetIframeGLOBAL.recommend.desc = rdesc;
                    owNetIframeGLOBAL.recommend.userGroup = ruserGroup;

                    //showRecommendationAsText(true);

                    $('#previousrecommendations_tbody').append('<tr><td>' + rtitle + '</td><td>' + ruserGroup + '</td></tr>');
                    $('#previousrecommendations').show();

                    $('#messages').html('<div class="message_success">Recommendation<strong> Updated</strong>!</div>');
                }

                else {
                    showError("We are sorry, some error occurred.<br />Is your OwNet Client connected to OwNet Server?");
                }
            });
        },
        complete: function () {
            hideLoader("td-recomm-submit", loader);
        }
    });
}

function editRecommendation() {
    showRecommendationAsForm();
}

function printTags(data) {
    var pagetags = "";
    $.each(data.tags, function (index, val) {
        pagetags += decodeURIComponent(val) + ", ";
    });
    $("#ttags").text(pagetags.replace(/,$/, ""));
}

function setTags() {
    var pagetags = $("#tags").val().replace(/[\^\$\W\.@]/g, " ").replace(/ +/g," ")
    if (pagetags.match(/\S/g) === null) {
        showError("You have to specify tags. Seperate them with space.");
        return;
    }
            
    if (pagetags !== null && pagetags.match(/\S/g) !== null && owNetIframeGLOBAL.tags !== pagetags) {
        var loader = showLoader("td-tag-submit");
        $.ajax
        ({
            type: "POST",
            url: "http://inject.ownet/tag/create",
            data: { page: decodeURIComponent(getEncodedParentPageUri()), tags: pagetags },
            dataType: 'json',
            success: function (data) {
                //$("#ttext").html("You've tagged this page.");
                printTags(data);
                owNetIframeGLOBAL.tags = pagetags;
            },
            complete: function () {
                hideLoader("td-tag-submit", loader);
            }
        });
    }
}

function getTags() {
    var loader = showLoader("populartags");
    $.ajax({
        type: "GET",
        // tu nastavime spravnu hlasovaciu url
        url: "http://inject.ownet/tag/read?page=" + getEncodedParentPageUri(),
        dataType: 'json',
        success: function (data) {
            printTags(data);
        },
        complete: function() {
            hideLoader("populartags", loader);
        }
    });
}

function login() {
    var loader = showLoader("login-loader");
    $.ajax({
        type: "POST",
        url: "http://inject.ownet/user/login",
        data: { username: document.getElementById("username").value, password: document.getElementById("password").value },
        dataType: 'json',
        success: function (data) {
            $.each(data, function (key, val) {
                if (key === "status") {
                    if (val === "OK") window.location.reload();
                    else document.getElementById("login-message").style.display = "block";
                }
            });
        },
        complete: function () {
            hideLoader("login-loader", loader);
        }
    });
}

function logout() {
    // TODO loading indicator
    $.ajax({
        type: "POST",
        url: "http://inject.ownet/user/logout",
        dataType: 'json',
        success: function (data) {
            window.location.reload();
        }
    });
}

/* Pri otvoreni stranky */
$(document).ready(function () {
    // qTip - zobrazenie napovedy 
    jQuery('.owe-tips[title]').qtip({
        content: {
            text: false
        },
        position: {
            my: 'right middle',
            at: 'left center'
        },
        style: {
            tip: true,
            classes: 'ui-tooltip-owe-tips'
        }
    });


    /* Novinka */
    $('.tip_help[title]').qtip({
        content: {
            text: false
        },
        position: {
            my: 'bottom left',
            at: 'top middle'
        },
        style: {
            tip: true,
            classes: 'ui-tooltip-owe-help'
        }
    });
});

function confirm_message(message, action) {
    if (confirm(message)) {
        action();
    }
}

function baseUrl() {
    return "http://inject.ownet/";
}