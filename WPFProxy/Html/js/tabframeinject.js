owNetAVAILABLEURIS = [];

owNetGLOBAL = {
    localUri: "http://inject.ownet/",
    homeUri: "http://my.ownet/",
    encodedPageUri: encodeURIComponent(document.location.href.replace(/#.*$/, "")),
    encodedReferrerUri: encodeURIComponent(document.referrer.replace(/#.*$/, "")),
    pageUriChanged: function () {
        var encuri = encodeURIComponent(document.location.href.replace(/#.*$/, ""));
        if (this.encodedPageUri !== encuri) {
            this.encodedPageUri = encuri;
            return true;
        }
        return false;
    },
    updatePageUri: function () {
        this.encodedPageUri = encodeURIComponent(document.location.href.replace(/#.*$/, ""));
    },
    clearNode: function (elem) {
        while (elem && elem.hasChildNodes()) {
            if (elem.firstChild.hasChildNodes())
                this.clearNode(elem.firstChild);
            else
                elem.clearChild(elem.firstChild);
        }
    },
    removeNode: function (node, parent) {
        while (node && node.hasChildNodes()) {
            this.removeNode(node.firstChild, node);
        }
        if (node && parent) {
            parent.removeChild(node);
        }
    },
    ajaxGet: function (url, callback) {
        var xmlHttp = null;
        try {
            xmlHttp = new XMLHttpRequest();
        }
        catch (e) {
            try {
                xmlHttp = new ActiveXObject("Msxml2.XMLHTTP");
            }
            catch (e) {
                xmlHttp = new ActiveXObject("Microsoft.XMLHTTP");
            }
        }

        if (xmlHttp == null) {
            return false;
        }
        xmlHttp.onreadystatechange = function () {
            callback(xmlHttp);
        };
        xmlHttp.open("GET", url, true);
        xmlHttp.send();
        return true;
    },
    loadScript: function (url, callback) {
        var head = document.getElementsByTagName('head')[0];
        var script = document.createElement('script');
        script.type = 'text/javascript';
        script.src = url;

        var callback_called = 0;
        if (document.all) {
            script.onreadystatechange = function () {
                if ((script.readyState === "loaded" || script.readyState === "complete") && callback_called == 0) {
                    callback();
                    callback_called = 1;
                }
            };
        }
        else {
            script.onload = function () {
                callback();
                callback_called = 1;
            };
        }

        head.appendChild(script);
    },
    loadCss: function (url) {
        var head = document.getElementsByTagName("head")[0];
        var css = document.createElement("link");
        css.setAttribute("rel", "stylesheet");
        css.setAttribute("type", "text/css");
        css.setAttribute("href", url);
        css.setAttribute("media", "screen");
        head.appendChild(css);
    },
    addCss: function (cssCode) {
        var styleElement = document.createElement("style");
        styleElement.type = "text/css";

        if (styleElement.styleSheet) {
            styleElement.styleSheet.cssText = cssCode;
        }
        else {
            styleElement.appendChild(document.createTextNode(cssCode));
        }
        document.getElementsByTagName("head")[0].appendChild(styleElement);
    },
    isArray: function () {
        if (arguments && typeof arguments[0] === "object") {
            var criterion = arguments[0].constructor.toString().match(/array/i);
            return (criterion != null);
        }
        return false;
    },
    urlEquals: function (a, b) {
        return (a.replace(/\//g, "") === b.replace(/\//g, ""));
    },
    fadeOut: function (target, callback) {
        var step = 10;
        var call = (arguments && arguments.length > 1) ? callback : null;
        target.style.opacity = 1.0;

        var anim = function () {
            if (step > 0) {
                step -= 2;
                target.style.opacity = step / 10.0;
                setTimeout(anim, 50);
            }
            else {
                target.style.display = "none";
                if (call !== null)
                    call();
            }
        };
        anim();
    },
    fadeIn: function (target, callback) {
        var step = 0;
        var call = (arguments && arguments.length > 1) ? callback : null;
        target.style.opacity = 0.0;
        target.style.display = "block";

        var anim = function () {
            if (step < 10) {
                step += 2;
                target.style.opacity = step / 10.0;

                setTimeout(anim, 50);
            }
            else {
                if (call !== null)
                    call();
            }
        };
        anim();
    },
    HighlightSwitch:
    {
        isSwitchedOn: 0,
        switchObj: null,
        availableUris: null,
        highlightedLinks: [],
        receiveLinks: function (linksobj) {
            if (owNetGLOBAL.isArray(linksobj)) {
                this.availableUris = linksobj;
                for (var i = 0; i < this.availableUris.length; ++i) {
                    this.availableUris[i] = decodeURIComponent(this.availableUris[i]);
                }
            }

        },
        doSwitch: function () {
            if (this.isSwitchedOn == 0) {  /* switch on */
                owNetAVAILABLEURIS = null;  // check the client everytime
                this.availableUris = null; 
                if (this.availableUris === null || this.availableUris.length === 0 || owNetGLOBAL.pageUriChanged() === true) {
                    owNetGLOBAL.updatePageUri();
                    this.availableUris = null;
                    this.highlightedLinks = [];
                    owNetGLOBAL.loadScript(owNetGLOBAL.localUri + "cache/links?page=" + owNetGLOBAL.encodedPageUri + "&gid=" + Math.floor((Math.random() * 1000) + 1), function () {
                        owNetGLOBAL.HighlightSwitch.receiveLinks(owNetAVAILABLEURIS); owNetGLOBAL.HighlightSwitch.switchOn();
                    }
                    );
                    return;
                }
                this.switchOn();
            }
            else {  /* switch off*/
                this.switchOff();
            }
        },
        switchOn: function () {
            for (var i = 0; i < document.links.length; ++i) {
                if (this.availableUris.owNetContains(document.links[i].href, owNetGLOBAL.urlEquals)) {
                    document.links[i].className += " owNetHighlight";
                    this.highlightedLinks[this.highlightedLinks.length] = document.links[i];
                }
            }
            this.isSwitchedOn = 1;
            this.switchObj.switchOn();
        },
        switchOff: function () {
            var reg = new RegExp("(\\s|^)" + "owNetHighlight" + "(\\s|$)");

            for (var i = 0; i < this.highlightedLinks.length; ++i) {
                this.highlightedLinks[i].className = this.highlightedLinks[i].className.replace(reg, "");
            }
            this.highlightedLinks = [];
            this.switchObj.switchOff();
            this.isSwitchedOn = 0;
        },
        assignSwitch: function (obj) {
            this.switchObj = obj;
            return function () {
                owNetGLOBAL.HighlightSwitch.doSwitch();
            };
        }
    },
    FrameSwitch:
    {
        isSwitchedOn: 0,
        switchObjs: [],
        frameDivElem: null,
        switchType: 0,
        previousSwitchType: 0,
        frameElem: null,
        doSwitch: function () {
            if (this.isSwitchedOn == 0) {  /*switch on*/
                this.switchOn();
            }
            else {  /* switch off*/
                this.switchOff();
            }
        },
        switchOn: function () {
            if (this.frameDivElem == null) return;
            if (this.switchType == 0)
                file_name = "index.html"
            else
                file_name = "caching.html";

            if (this.previousSwitchType != this.switchType || this.frameElem === null || owNetGLOBAL.pageUriChanged() === true) {
                owNetGLOBAL.updatePageUri();
                if (this.frameElem !== null) {
                    owNetGLOBAL.removeNode(this.frameElem, this.frameDivElem);
                    this.frameElem = null;
                }
                /*<iframe src="iframe.htm" width="600" height="600" scrolling="no" frameborder="0"></iframe>*/
                var div_iframe = document.createElement('iframe');
                div_iframe.setAttribute("src", owNetGLOBAL.localUri + file_name + "?parent=" + owNetGLOBAL.encodedPageUri);
                div_iframe.setAttribute("width", "460");
                div_iframe.setAttribute("height", "600");
                div_iframe.setAttribute("id", "owNet-iframe");
                div_iframe.setAttribute("scrolling", "auto");
                div_iframe.setAttribute("frameborder", "0");
                this.frameElem = div_iframe;
                this.frameDivElem.appendChild(div_iframe);
            }

            this.frameDivElem.style.display = "block";
            this.isSwitchedOn = 1;
            this.previousSwitchType = this.switchType
            for (var i = 0; i < this.switchObjs.length; ++i) this.switchObjs[i].switchOn();
        },
        switchOff: function () {
            if (this.frameDivElem == null) return;
            this.frameDivElem.style.display = "none";
            for (var i = 0; i < this.switchObjs.length; ++i) this.switchObjs[i].switchOff();
            this.isSwitchedOn = 0;
        },
        assignSwitch: function (obj, div) {
            var that = this;
            this.switchObjs[this.switchObjs.length] = obj;
            this.frameDivElem = div;
            return function () {
                that.doSwitch();
            };
        }
    },

    Notifier: {
        Recomms: function () {
            owNetGLOBAL.loadScript(owNetGLOBAL.localUri + "recommend/js_not_displayed" + "?gid=" + Math.floor((Math.random() * 1000) + 1), function () {
                var notif = document.getElementById("notification");
                /* if (owNetNEWRECOMMS > 99)
                owNetNEWRECOMMS = "99+"*/
                if (owNetNEWRECOMMS != 0) {
                    notif.setAttribute("class", "owe-notificator_ac");
                    if (owNetNEWRECOMMS > 99)
                        notif.innerHTML += "<strong>99+<strong>";
                    else
                        notif.innerHTML += "<strong>" + owNetNEWRECOMMS + "<strong>";
                }
                else {
                    notif.setAttribute("class", "owe-notificator");
                    notif.innerHTML += "<strong>" + owNetNEWRECOMMS + "<strong>";
                }

            });
        }
    },
    ProxyContact:
    {
        hasReportedPrimary: 0,
        hasReportedPrefetch: 0,
        hasReportedClose: 0,
        isRefreshed: 0,
        reportPrimary: function () {
            try {
                if (this.hasReportedPrimary == 0) {
                    owNetGLOBAL.loadScript(owNetGLOBAL.localUri + "history/create?page=" + owNetGLOBAL.encodedPageUri + "&ref=" + owNetGLOBAL.encodedReferrerUri + "&gid=" + Math.floor((Math.random() * 1000) + 1), function () {
                        return true;
                    });
                    this.hasReportedPrimary = 1;
                }
                else {
                    owNetGLOBAL.loadScript(owNetGLOBAL.localUri + "history/read?page=" + owNetGLOBAL.encodedPageUri + "&gid=" + Math.floor((Math.random() * 1000) + 1), function () {
                        return true;
                    });
                    this.hasReportedPrimary += 1;
                }
                this.startReportTimeout();
            }
            catch (e) {
            }
        },
        reportPrefetch: function (from) {
            try {
                if (from === "proxy" || from === "server") {
                    if (this.hasReportedPrefetch == 0) {
                        var target = owNetGLOBAL.encodedReferrerUri;
                        if (target === null || target.match(/\S/g) === null)
                            target = owNetGLOBAL.encodedPageUri;
                        owNetGLOBAL.loadScript(owNetGLOBAL.localUri + "prefetch/done?page=" + target + "&for=" + from + "&gid=" + Math.floor((Math.random() * 1000) + 1), function () {
                            return true;
                        });
                        this.hasReportedPrefetch = 1;
                    }
                }
            }
            catch (e) {
            }
        },


        reportClose: function () {
            try {
                if (this.hasReportedClose == 0) {
                    /*         owNetGLOBAL.loadScript(owNetGLOBAL.localUri + "prefetch/cancel?page=" + owNetGLOBAL.encodedPageUri, function () {
                    return true;
                    });*/
                    owNetGLOBAL.ajaxGet(owNetGLOBAL.localUri + "prefetch/cancel?page=" + owNetGLOBAL.encodedPageUri, function (xmlHttp) {
                        if (xmlHttp.readyState == 4 && xmlHttp.status == 200) {
                            return true;
                        }
                        return true;
                    });
                    this.hasReportedClose = 1;
                }
            }
            catch (e) {
            }
        },
        startRefresh: function () {
            owNetGLOBAL.loadScript(owNetGLOBAL.localUri + "refresh/start", function () {
                window.location.reload();
            });
        },
        stopRefresh: function () {
            try {
                this.stopRefreshTimeout();
                if (this.isRefreshed == 0) {
                    owNetGLOBAL.loadScript(owNetGLOBAL.localUri + "refresh/stop", function () {
                        return true;
                    });
                    this.isRefreshed = 1;
                }
            }
            catch (e) {
                throw e;
            }
        },
        refreshTimeout: 0,
        reportTimeout: 0,
        startRefreshTimeout: function () {
            this.refreshTimeout = setTimeout(this.stopRefresh, 1000 * 60 * 2);
        },
        stopRefreshTimeout: function () {
            if (this.refreshTimeout) clearTimeout(this.refreshTimeout);
        },
        startReportTimeout: function () {
            this.reportTimeout = setTimeout(this.reportPrimary, 1000 * 60 * 2);
        },
        stopReportTimeout: function () {
            if (this.reportTimeout) clearTimeout(this.reportTimeout);
        }
    }

};



Array.prototype.owNetContains = function (item, comparer) {
    var compfunc = (comparer && typeof comparer === "function") ? comparer : null;
    if (item)
    {
        for (var i = 0; i < this.length; ++i)
        {
            if (compfunc != null)
            {
                if (compfunc(this[i], item)) 
                     return true;
            }
            else if (this[i] === item)
                return true;
        }
    }
    return false;
};


(function () {

     var text = document.getElementById('notification');
     
     
    try 
    {

        if (top === self) 
        {
            if (document.referrer.match(/proxy.ownet\/prefetch/) !== null) 
            {
                
                if (document.addEventListener) 
                {   
                    document.addEventListener("DOMContentLoaded", function () {
                        document.removeEventListener("DOMContentLoaded", arguments.callee, false);
                        owNetGLOBAL.ProxyContact.reportPrefetch("proxy");
                    }, false);
                }
                else if (document.attachEvent) 
                {   
                    document.attachEvent("onreadystatechange", function () {
                        if (document.readyState === "complete") 
                        {
                            document.detachEvent("onreadystatechange", arguments.callee);
                            owNetGLOBAL.ProxyContact.reportPrefetch("proxy");
                        }
                    });
                }
            }
            else if (document.referrer.match(/server.ownet\/prefetch/) !== null) 
            {
                if (document.addEventListener) 
                {   
                    document.addEventListener("DOMContentLoaded", function () {
                        document.removeEventListener("DOMContentLoaded", arguments.callee, false);
                        owNetGLOBAL.ProxyContact.reportPrefetch("server");
                    }, false);
                }
                else if (document.attachEvent) 
                {   
                    document.attachEvent("onreadystatechange", function () {
                        if (document.readyState === "complete") 
                        {
                            document.detachEvent("onreadystatechange", arguments.callee);
                            owNetGLOBAL.ProxyContact.reportPrefetch("server");
                        }
                    });
                }
            }
            else 
            {
                var root = document.getElementById("owNet-div");
                if (root) 
                {
                    owNetGLOBAL.loadCss(owNetGLOBAL.localUri + "css/tab.css");

                    /* <div style="position: fixed !important; margin: 30px 0px -10px 10px; border: 3px solid #c4c4c4">*/
                    var root_div = document.createElement('div');
                    root_div.setAttribute("style", "position: absolute !important; text-align:right; left:0px; top: 30px;border: 3px solid #c4c4c4 !important; display: none; z-index: 999999 !important; background: rgb(255, 244, 190)");
                    root_div.setAttribute("id", "owNet-iframe-div");
                    root.appendChild(root_div);
                    var div_div = document.createElement("div");
                    div_div.id = "owNet-iframe-close";
                    root_div.appendChild(div_div);
                    var div_img = document.createElement("img");
                    div_img.src = owNetGLOBAL.localUri + "graphics/close.png";
                    div_img.setAttribute("style", "margin-top: 3px; margin-right: 3px; cursor: pointer; border: 0px;");
                    div_img.alt = "Close OwNet box";
                    div_img.title = "Close OwNet box";
                    div_div.appendChild(div_img);
                    div_img.onclick = owNetGLOBAL.FrameSwitch.assignSwitch(
                    {
                        switchOn: function () { }, 
                        switchOff: function () { } 
                    },
                    document.getElementById("owNet-iframe-div"));
                    /*</div>*/

                    /* <div id="owe-tab" style="display: none;">*/
                    root_div = document.createElement("div");
                    root_div.setAttribute("id", "owe-tab");
                    /*root_div.setAttribute("style", "display: none;");*/
                    root.appendChild(root_div);
                    /* <div class="my-owe">*/
                    div_div = document.createElement("div");
                    div_div.setAttribute("class", "my-owe");
                    root_div.appendChild(div_div);
                    /* <a href="">*/
                    /*var div_a = document.createElement("a");
                    div_a.setAttribute("href", "javascript:void(0);");
                    div_div.appendChild(div_a);    
                     <img src="graphics/notificator.png" class="img-4 owe-tips" alt="News"/>
                    var a_img = document.createElement("img");
                    a_img.setAttribute("src", owNetGLOBAL.localUri + "graphics/notificator.png");
                    a_img.setAttribute("class", "img-4 owe-tips");
                    a_img.setAttribute("alt", "News");
                    div_a.appendChild(a_img);*/
                    /* </a>*/
                    /* <a href="">*/
                    var div_a = document.createElement("a");
                    div_a.setAttribute("href", owNetGLOBAL.homeUri);
                    div_div.appendChild(div_a);
                    /* <img src="graphics/owetab_home.png" class="img-4 owe-tips" title="" alt="Home"/>*/
                    a_img = document.createElement("img");
                    a_img.setAttribute("src", owNetGLOBAL.localUri + "graphics/owetab_home.png");
                    a_img.setAttribute("class", "img-4 owe-tips");
                    a_img.setAttribute("alt", "Home");
                    a_img.setAttribute("title", "Visit OwNet homepage.");
                    div_a.appendChild(a_img);
                    /* </a>*/

                     /* <a href="">*/
                    
                    var div_a = document.createElement("a");

                    notifikacia = document.createElement("span");
                    notifikacia.setAttribute("id","notification");

                    div_a.setAttribute("href", "http://my.ownet/recommend/");
                    div_a.setAttribute("target", "_blank");
                    div_a.appendChild(notifikacia);
                    div_div.appendChild(div_a);
                    /* <img src="graphics/owetab_on.png" class="img-4 owe-tips" title="" alt="On"/>*/
                    notifikacia.setAttribute("title", "Notifies, if there is any new recommendation.");
                  
                    
                    owNetGLOBAL.Notifier.Recomms();

                    /* </a>*/




                    /* <a href="">*/
                    div_a = document.createElement("a");
                    div_a.setAttribute("href", "javascript:void(0);");
                    div_a.onclick = function () { owNetGLOBAL.ProxyContact.startRefresh(); return false; };
                    div_div.appendChild(div_a);
                    /* <img src="graphics/owetab_refresh.png" class="img-4 owe-tips" title="" alt="Refresh"/>*/
                    a_img = document.createElement("img");
                    a_img.setAttribute("src", owNetGLOBAL.localUri + "graphics/owetab_refresh.png");
                    a_img.setAttribute("class", "img-4 owe-tips");
                    a_img.setAttribute("alt", "Refresh");
                    a_img.setAttribute("title", "Download new version of current webpage.");
                    div_a.appendChild(a_img);
                    /* </a>*/


                    var a_strong = document.createElement("strong");
                    strong_text_show = document.createTextNode("Show OwNet");
                    strong_text_hide = document.createTextNode("Hide OwNet");

                    var switchOnFunc = function () {
                        if (this.switched == 0) {
                            a_strong.removeChild(strong_text_show); a_strong.appendChild(strong_text_hide); this.switched = 1;
                        } 
                    };
                    var switchOffFunc = function () {
                        if (this.switched == 1) 
                        { 
                            a_strong.removeChild(strong_text_hide); a_strong.appendChild(strong_text_show); this.switched = 0; 
                        } 
                    };

                    /*Caching popup*/
                    /* <a href="">*/
                    div_a = document.createElement("a");
                    div_a.setAttribute("href", "javascript:void(0);");
                    div_div.appendChild(div_a);
                    /* <img src="graphics/owetab_star.png" class="img-4 owe-tips" title="" alt="Recommend"/>*/
                    a_img = document.createElement("img");
                    a_img.setAttribute("src", owNetGLOBAL.localUri + "graphics/owetab_caching.png");
                    a_img.setAttribute("class", "img-4 owe-tips");
                    a_img.setAttribute("alt", "Cache");
                    a_img.setAttribute("title", "Open options to configure caching.");
                    div_a.appendChild(a_img);
                    /* Show more      */
                    div_a.onclick = function () {
                        owNetGLOBAL.FrameSwitch.switchType = 1;
                        (owNetGLOBAL.FrameSwitch.assignSwitch({
                            switched: 0,
                            switchOn: switchOnFunc,
                            switchOff: switchOffFunc
                        }, document.getElementById("owNet-iframe-div")))();
                    };
                    /* </a>*/

                    /* <a href="">*/
                    div_a = document.createElement("a");
                    div_a.setAttribute("href", "javascript:void(0);");
                    div_div.appendChild(div_a);
                    /* <img src="graphics/owetab_on.png" class="img-4 owe-tips" title="" alt="On"/>*/
                    var a_img_switch = document.createElement("img");
                    a_img_switch.setAttribute("src", owNetGLOBAL.localUri + "graphics/owetab_off.png");
                    a_img_switch.setAttribute("class", "img-4 owe-tips");
                    a_img_switch.setAttribute("alt", "Highlighting");
                    a_img_switch.setAttribute("title", "Highlight links on this webpage which are available offline.");
                    div_a.appendChild(a_img_switch);
                    div_a.onclick = owNetGLOBAL.HighlightSwitch.assignSwitch(
                    { 
                        switchOn: function () { a_img_switch.setAttribute("src", owNetGLOBAL.localUri + "graphics/owetab_on.png"); },
                        switchOff: function () { a_img_switch.setAttribute("src", owNetGLOBAL.localUri + "graphics/owetab_off.png"); } 
                    } 
                    );
                    /* </a>*/


                    /* <a href="">*/
                    div_a = document.createElement("a");
                    div_a.setAttribute("href", "javascript:void(0);");
                    div_div.appendChild(div_a);
                    /* <img src="graphics/owetab_star.png" class="img-4 owe-tips" title="" alt="Recommend"/>*/
                    a_img = document.createElement("img");
                    a_img.setAttribute("src", owNetGLOBAL.localUri + "graphics/owetab_star.png");
                    a_img.setAttribute("class", "img-4 owe-tips");
                    a_img.setAttribute("alt", "Recommend");
                    a_img.setAttribute("title", "Open OwNet box for rating, recommending and tagging this webpage.");
                    div_a.appendChild(a_img);
                    /* Show more*/
                    a_strong.appendChild(strong_text_show);
                    div_a.appendChild(a_strong);
                    div_a.onclick = function () 
                    {
                        owNetGLOBAL.FrameSwitch.switchType = 0;
                        (owNetGLOBAL.FrameSwitch.assignSwitch({
                            switched: 0,
                            switchOn: switchOnFunc,
                            switchOff: switchOffFunc
                        }, document.getElementById("owNet-iframe-div")))();
                    };
                     /*</a>
                     </div>
                    <div class="owe-tab-close">*/
                    div_div = document.createElement("div");
                    div_div.setAttribute("class", "owe-tab-close");
                    root_div.appendChild(div_div);
                    /*<img src="graphics/close.png" alt="Close" title="Close owNet" />*/
                    div_img = document.createElement("img");
                    div_img.setAttribute("src", owNetGLOBAL.localUri + "graphics/close.png");
                    div_img.setAttribute("alt", "Close");
                    div_img.setAttribute("title", "Close OwNet bar and box.");
                    div_img.onclick = function () { owNetGLOBAL.FrameSwitch.switchOff(); owNetGLOBAL.fadeOut(root_div); };
                    div_div.appendChild(div_img);
                    /* </div>
                     </div>*/

                    /* report this page as primary*/
                    owNetGLOBAL.ProxyContact.reportPrimary();

                    owNetGLOBAL.ProxyContact.startRefreshTimeout();

                    if (document.addEventListener) 
                    {   
                        document.addEventListener("DOMContentLoaded", function () 
                        {
                            document.removeEventListener("DOMContentLoaded", arguments.callee, false);
                            owNetGLOBAL.ProxyContact.stopRefresh();
                        }, false);
                    }
                    else if (document.attachEvent) 
                    {  
                        document.attachEvent("onreadystatechange", function () 
                        {
                            if (document.readyState === "complete") 
                            {
                                document.detachEvent("onreadystatechange", arguments.callee);
                                owNetGLOBAL.ProxyContact.stopRefresh();
                            }
                        });
                    }

                    if (window.addEventListener) 
                    {   
                        window.addEventListener("unload", function () 
                        {
                            owNetGLOBAL.ProxyContact.reportClose();
                        }, false);
                        window.addEventListener("beforeunload", function () 
                        {
                            owNetGLOBAL.ProxyContact.reportClose();
                        }, false);
                    }
                    else if (window.attachEvent) 
                    {   
                        window.attachEvent("onunload", function () 
                        {
                            owNetGLOBAL.ProxyContact.reportClose();
                        });
                        window.attachEvent("onbeforeunload", function () 
                        {
                            owNetGLOBAL.ProxyContact.reportClose();
                        });
                    }
                }
                else 
                {
                    owNetGLOBAL.removeNode(root, root.parentNode);
                }
            }
        }
    }
    catch (e) 
    {
        throw e;
    }
})();

