function NewWindow(weblink)
{
	return window.open(weblink.href, "_blank");
}

/* Ownet Tooltip */
function OwnetTooltip(section) {
    // pomale otvorenie tooltipu, ak je dovolene
    if ($.cookie('cookie_' + section) != "close")
        $('#ownet-tooltip').fadeIn(2000);

    // zatvorenie tooltipu s cookies
    $('#ownet-tooltip .ownet-tooltip-close').on('click', function () {
        $(this).parent().fadeOut(1000);

        $.cookie('cookie_' + section, 'close', { expires: 365 });
    });
}

$('.user-ownet li').hover(function () { $('ul', this).slideDown(100); }, function () { $('ul', this).slideUp(100); });



/* Zmeni velkost pisma podla potreby */
function resizeText(idCss, size) {

    // vyparsujeme velkost pisma
    size = parseInt(size, 10);
    var element; var curFont;

    // ziskame pozadanove ID
    element = document.getElementById(idCss);

    // ziskame akutalnu velkost pisma elementu
    curFont = parseInt(element.style.fontSize, 10);

    // zmenime novu velkost elementu
    element.style.fontSize = (curFont + size) + 'px';

    return;
}


function showLoader(rootId) {
    var root = null;
    if (typeof(rootId) === "string") 
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
$(document).ready(function () {
    $('.nav li').hover(
		function () {
		    //show submenu
		    $('ul', this).slideDown(100);
		},
		function () {
		    // hide submenu
		    $('ul', this).slideUp(100);
		}
	);
});

$(document).ready(function () {
    $("#tab-fancy-login").fancybox({
        'titlePosition': 'inside',
        'transitionIn': 'none',
        'transitionOut': 'none'
    });

});


function login() {
	var loader = showLoader("login-loader");
	$.ajax({
	    type: "POST",
	    url: "http://my.ownet/user/login",
	    data: { username: document.getElementById("loginusername").value, password: document.getElementById("loginpassword").value },
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
        url: "http://my.ownet/user/logout",
        dataType: 'json',
        success: function (data) {
            parent.location.href = "http://my.ownet/";
        }
    });
}

/* Open user tab */
function UserInfo() {
    $('.user-ownet li ul').toggle('100', function () {

    });
}	

function deleteRecommendation(rid, gid, obj) {
//    var loader = showLoader(obj.parentNode);

    $.ajax({
        type: "POST",
        url: "http://my.ownet/recommend/delete",
        dataType: 'json',
        data: { id : rid, group : gid },
        success: function (data) {
            $.each(data, function (key, val) {
                if (key === "status" && val === "OK") {
                    $(obj.parentNode.parentNode).fadeOut(1000);
                }
            });
        }//,
  //      complete: function () {
    //        hideLoader(obj.parentNode, loader);
      //  }
    });
}
function deleteActivity(aid, obj) {
    //var loader = showLoader(obj.parentNode);
    $.ajax({
        type: "POST",
        url: "http://my.ownet/activity/delete",
        dataType: 'json',
        data: { id: aid },
        success: function (data) {
            $.each(data, function (key, val) {
                if (key === "status" && val === "OK") {
                    $(obj.parentNode.parentNode).fadeOut(1000);
                }
            });
        } //,
        //complete: function () {
        //  hideLoader(obj.parentNode, loader);
        //}
    });
}
function deleteHistory(hid, obj) {
    //var loader = showLoader(obj.parentNode);
    $.ajax({
        type: "POST",
        url: "http://my.ownet/history/delete",
        dataType: 'json',
        data: { id: hid },
        success: function (data) {
            $.each(data, function (key, val) {
                if (key === "status" && val === "OK") {
                    $(obj.parentNode.parentNode).fadeOut(1000);
                }
            });
        } //,
        //complete: function () {
        //  hideLoader(obj.parentNode, loader);
        //}
    });
}
function deleteUser(userId) {
    $.ajax({
        type: "POST",
        url: "http://my.ownet/user/delete",
        dataType: 'json',
        data: { id: userId },
        success: function (data) {
            $.each(data, function (key, val) {
                if (key === "status" && val === "OK") {
                    $("#user" + userId).fadeOut(1000);
                }
            });
        }
    });
}

function deleteOrder(orderId) {
    $.ajax({
        type: "POST",
        url: "http://my.ownet/prefetch/delete",
        dataType: 'json',
        data: { id: orderId },
        success: function (data) {
            $.each(data, function (key, val) {
                if (key === "status" && val === "OK") {
                    $("#order" + orderId).fadeOut(1000);
                }
            });
        }
    });
}

function deleteTag(tagid,obj) {
    $.ajax({
        type: "POST",
        url: "http://my.ownet/tag/delete",
        dataType: 'json',
        data: { tag: tagid },
        success: function (data) {
            $.each(data, function (key, val) {
                if (key === "status" && val === "OK") {
                    $(obj.parentNode).fadeOut(1000);
                }
            });
        }
    });
}

function useRecommendation(obj) {
    return reportUsage("http://my.ownet/activity/create/recommend", obj);
}

function useRating(obj) {
    return reportUsage("http://my.ownet/activity/create/rating", obj);
}

function useHistory(obj) {
    return reportUsage("http://my.ownet/activity/create/history", obj);
}

function useSearch(obj) {
    return reportUsage("http://my.ownet/activity/create/search", obj.href);
}

function useShared(obj) {
    return reportUsage("http://my.ownet/activity/create/share", obj.href);
}


function reportUsage(localurl, pageurl) {
    $.ajax({
            type: "POST",
            url : localurl,
            dataType: "json",
            data : { page : pageurl }
    });
    return true;
}

function updateInformation() {
    var loader = showLoader("infoloader");
    $.ajax({
        type: 'post',
        url: "http://my.ownet/user/information/update",
        data: { userid: $("#userid").val(), username: $("#username").val(), firstname: $("#firstname").val(), surname: $("#surname").val(), firstname: $("#firstname").val(), gender: $('input[name=gender]:checked', '#updateform').val(), role: ($('input[name=role]','#updateform').length>0  ? $('input[name=role]:checked', '#updateform').val() : ""), email: $("#email").val() },
        success: function (data) {
            $("#userinfo").empty().append(data);
        },
        dataType: 'html',
        complete: function () {
            hideLoader("infoloader", loader);
        }
    });
}
function updatePassword() {
    var loader = showLoader("passloader");
    $.ajax({
        type: 'post',
        url: "http://my.ownet/user/password/update",
        data: { userid: $("#userid").val(), username: $("#username").val(), password0: $("#password0").length > 0 ? $("#password0").val() : "", password1: $("#password1").val(), password2: $("#password2").val() },
        success: function (data) {
            $("#userpass").empty().append(data);
        },
        dataType: 'html',
        complete: function () {
            hideLoader("passloader", loader);
        }
    });
}

function hideDeleteTag(span) {
    if (span.hasChildNodes()) {
        span.lastChild.style.display = "none";
    }
}

function showDeleteTag(span) {
    if (span.hasChildNodes()) {
        span.lastChild.style.display = "inline";
    }
}



function confirm(message, action) {
    if ($("#confirmdiv").length <= 0) {
    
        var div = document.createElement("div");
        div.id = "confirmdiv";
        document.body.appendChild(div);
        var h2 = document.createElement("h2");
        div.appendChild(h2);
        h2.innerHTML = "Confirmation";
        var p = document.createElement("p");
        p.id = "confirmmsg";
        div.appendChild(p);
        p = document.createElement("div");
        p.setAttribute("style", "text-align: center;");
        var but = document.createElement("input");
        but.type = "button";
        but.className="button_login";
        but.value = "Yes";
        but.setAttribute("style", "margin-right: 10px;");
        but.id = "confirmyes";
        div.appendChild(p);
        p.appendChild(but);
        var but = document.createElement("input");
        but.type = "button";
        but.className = "button_login";
        but.setAttribute("style", "margin-left: 10px;");
        but.value="No";
        but.onclick = function() { parent.$.fancybox.close(); };
        p.appendChild(but);
    }

    document.getElementById("confirmmsg").innerHTML = message;
    document.getElementById("confirmyes").onclick = function() { parent.$.fancybox.close(); action(); }

    $.fancybox(
        {
	        'autoDimensions': false,
	        'width': 350,
            'content': $("#confirmdiv"),
	        'height': 'auto',
	        'transitionIn': 'none',
	        'transitionOut': 'none',
	        'showCloseButton': false,
	        'hideOnContentClick': false
	    }
    );
}

function confirm_message(message, action) {
    confirm(message, action);
}

function baseUrl() {
    return "http://my.ownet/";
}
