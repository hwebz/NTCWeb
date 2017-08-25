<%@ Page Language="c#" AutoEventWireup="false" Inherits="BVNetwork.FileNotFound.NotFoundBase" %>

<%-- 
    Note! This file has no code-behind. It inherits from the NotFoundBase class. You can 
    make a copy of this file into your own project, change the design and keep the inheritance 
    WITHOUT having to reference the BVNetwork.EPi404.dll assembly.
    
    If you want to use your own Master Page, inherit from SimplePageNotFoundBase instead of
    NotFoundBase, as that will bring in what is needed by EPiServer. Note! you do not need to
    create a page type for this 404 page. If you use the EPiServer API, and inherit from  
    SimplePageNotFoundBase, this page will run in the context of the site start page.
    
    Be very careful with the code you write here. If you reference resources that cannot be found
    you could end up in an infinite loop.
    
    The code behind file might do a redirect to a new page based on the redirect configuration if
    it matches the url not found. The Error event (where the rest of the redirection is done)
    might not run for .aspx files that are not found, instead it redirects here with the url it
    could not find in the query string.
    
    Available properties:
        Content (BVNetwork.FileNotFound.Content.PageContent)
            // Labels you can use - fetched from the language file
            Content.BottomText
            Content.CameFrom
            Content.LookingFor
            Content.TopText
            Content.Title
            
        UrlNotFound (string)
            The url that cannot be found
        
        Referer (string)
            The url that brought the user here
            It no referer, the string is empty (not null)
            
    If you are using a master page, you should add this:
        <meta content="noindex, nofollow" name="ROBOTS">
    to your head tag for this page (NOT all pages)
 --%>

<script runat="server" type="text/C#">
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // Add your own logic (like databinding) here
    }
</script>

<%@ Register TagPrefix="EPiServer" Namespace="EPiServer.WebControls" Assembly="EPiServer" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<%--<html>
<head>
    <title><%= Content.Title %></title>
    <meta content="noindex, nofollow" name="ROBOTS" />
    <style type="text/css">
        body
        {
            font-family: Verdana, Arial, Helvetica, Tahoma;
            font-size: 0.65em;
            color: #333;
            background-color: #ffffff;
        }
        
        a
        {
            color: 0000cc;
        }
        a:hover
        {
            color: #000;
            text-decoration: none;
        }
        h1
        {
            font-weight: bold;
            font-size: 1.8em;
            color: #606060;
            margin-bottom: 0.5em;
            margin-top: 0.5em;
        }
        div.lookingfor
        {
            border: #660033 1px solid;
            padding: 10px;
            background-color: #ffdab5;
            width: 100%;
        }
        .notfoundbox
        {
            border-bottom: solid 1px #cccccc;
            border-right: solid 1px #cccccc;
            border-left: solid 1px #f8f8f8;
            border-top: solid 1px #f8f8f8;
            padding: 10px 10px 10px 10px;
            width: 100%;
            background-color: #f8f8f8;
            font-weight: bold;
            width: 100%;
        }
        .logo
        {
            font-family: Verdana;
            font-size: 5em;
            color: #a0a0a0;
            padding-bottom: 0.5em;
            letter-spacing: -0.08em;
        }
        div.floater
        {
            position: absolute; 
            bottom: 0; 
            right: 0;
            font-family: Times New Roman;
            font-size: 10em;
            font-style: italic;
            color: #f0f0f0;
            margin: 0 20px 10px 0;
            letter-spacing: -0.08em;
        }
    </style>
</head>
<body>
    <form id="FileNotFoundForm" method="post" runat="server">
    <div class="logo">
        Company Logo Here
    </div>
    <h1>
        <%= Content.Title %></h1>
    <div style="width: 760px">
        <div style="padding-left: 10px; float: left; width: 550px">
            <%= Content.TopText %>
            <%= Content.LookingFor %>
            <div class="notfoundbox">
                <%= UrlNotFound %>
                <%= Referer.Length > 0 ? Content.CameFrom : "" %>
                <%= Referer.Length > 0 ? Referer : "" %>
            </div>
            <%= Content.BottomText %>
        </div>
        <div style="padding-right: 10px; padding-left: 10px; float: right; width: 200px">
            &nbsp;
        </div>
    </div>
    <div class="floater">
        404
    </div>
    </form>
</body>
</html>--%>

<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>About us</title>
    
    <link rel="dns-prefetch" href="//niteco.com" />
    <link rel="dns-prefetch" href="https://maps.googleapis.com/" />
    <link rel="dns-prefetch" href="http://www.google-analytics.com/" />
    <link rel="dns-prefetch" href="http://vjs.zencdn.net/" />  
    
    <link rel="shortcut icon" href="/img/favicon.ico" type="image/x-icon" />
    <meta name="description" content="" />
    <meta property="og:description" />
    <link href="/css/screen.css" rel="stylesheet" media="screen" />
    <link href="/css/print.css" rel="stylesheet" media="print" />
     
    <script>
        (function (i, s, o, g, r, a, m) {
            i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () {
                (i[r].q = i[r].q || []).push(arguments)
            }, i[r].l = 1 * new Date(); a = s.createElement(o),
            m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)
        })(window, document, 'script', '//www.google-analytics.com/analytics.js', 'ga');

        ga('create', 'UA-12226497-33', 'niteco.com');
        ga('send', 'pageview');




</script>
</head>

<body>
    
   

    <div class="loading-container" data-require="base/loading">
        <div class="loading-mask">
        </div>
    </div>
    <div class="page-wrapper disable-scrolling">

<section class="top-navigator-container">
    <a class="top-navigotar__link" href="/" title="">
        <img class="top-navigator__logo" src="/globalassets/logo.png" alt="Diverse Minds, Creating World-class Software" />
    </a> 
</section>       


<div class="about-container content-container" data-require="base/about-us-page">
    <div class="about-content-wrapper content-wrapper">
        <section class="page-introduction-container">
		<div class="page-introduction-top-block" data-require="base/page-introduction">
               <!--  <div class="page-introduction-top-block__background page-introduction__background" >
                    <picture >
                        <source media="(min-width: 75rem)" srcset="/globalassets/header-new-images/desktop/not-found.jpg?preset=1900">
                        <source media="(min-width: 62rem)" srcset="/globalassets/header-new-images/desktop/not-found?preset=1300">
                        <source media="(min-width: 48rem)" srcset="/globalassets/header-new-images/tablet/not-found-tablet.jpg?preset=1000, /globalassets/header-new-images/tablet_2x/not-found-tablet_2x.jpg?preset=1900 2x">
                        <img src="/globalassets/header-new-images/mobile/not-found-mobile.jpg?preset=800" alt="Not found" />
                    </picture>
                </div> -->
                <div class="page-introduction-top-block__content page-introduction__content">
                    <h1>404 - Page Not Found</h1>
                    <span class="yellow-divider"></span>
                    <span class="page-introduction-top-block__content__sub-title"><p>The page you requested is not available</p></span>
                </div>
            </div>
        </section>  



<div class="footer-page" data-require="base/footer">
    <div class="container">
        <div class="footer-page__partner-container">
                    <span>
                        <img src="/globalassets/footer/footer_microsoft.png" />
                    </span>
                    <span>
                        <img src="/globalassets/footer/footer_episerver.png" />
                    </span>


        </div>

        <div class="footer-page__copyright">
            &copy; 2015 niteco.com
        </div>
        <div class="footer-page__social">
            <a class="footer-page__social__facebook" href="https://www.facebook.com/Niteco" title="" target="_blank"></a>
            <a class="footer-page__social__linkin" href="https://www.linkedin.com/company/niteco-vietnam-company-limited" title="" target="_blank"></a>
        </div>
    </div>
</div>
    </div>
</div>

    </div>




<div class="search-container" data-require="base/search" data-action-search="/SearchPage/GetSearchResult">
    
    <div class="close-button"></div>
    <div class="search-form">
        <input type="text" id="searchBox" class="search-box" placeholder="Search..." />
        <img class="img-loading" src="/img/ajax-loader.gif" alt="Loading" />
    </div>
    <div class="container-fluid search-result">
    </div>
    <div class="container-fluid search-default">
        <div class="row">
            <div class="col-xs-12 col-sm-6 col-lg-4 search-default__column">
                <h3>About Niteco</h3>
                                <a class="search-default__item search-default__location" href="/service-list/" title="">
                                    Our Services
                                </a>
                                <a class="search-default__item search-default__location" href="/technologies/" title="">
                                    Technologies
                                </a>
                                <a class="search-default__item search-default__location" href="/our-team/" title="">
                                    Our Team
                                </a>
                                <a class="search-default__item search-default__location" href="/engagement-models/" title="">
                                    Engagement Models
                                </a>
                                <a class="search-default__item search-default__location" href="http://charity.niteco.se/" title="">
                                    Nicef Charity
                                </a>

            </div>
            <div class="col-xs-12 col-sm-6 col-lg-4 search-default__column">
                <h3>Office Location</h3>
                                <a class="search-default__item search-default__location" href="/contact-us/" title="">
                                    Hanoi
                                </a>
                                <a class="search-default__item search-default__location" href="/contact-us/" title="">
                                    Ho Chi Minh City
                                </a>
                                <a class="search-default__item search-default__location" href="/contact-us/" title="">
                                    Stockholm
                                </a>
                                <a class="search-default__item search-default__location" href="/contact-us/" title="">
                                    Palm Desert
                                </a>
                                <a class="search-default__item search-default__location" href="/contact-us/" title="">
                                    Causeway Bay
                                </a>

            </div>
          
            <div class="col-xs-12 col-sm-6 col-lg-4 search-default__column">
                <h3>Job Openings</h3>
                                <a class="search-default__item search-default__location" href="/careers/senior-.net-developers/" title="">
                                    Senior .NET Developers
                                </a>
                                <a class="search-default__item search-default__location" href="/careers/senior-python-developer/" title="">
                                    Senior Python Developer
                                </a>
                                <a class="search-default__item search-default__location" href="/careers/php-project-manager/" title="">
                                    PHP Project Manager
                                </a>
                                <a class="search-default__item search-default__location" href="/careers/senior-graphic-designer/" title="">
                                    Senior Graphic Designer
                                </a>
                                <a class="search-default__item search-default__location" href="/careers/front-end-developer/" title="">
                                    Front-end Developer
                                </a>

            </div>
        </div>
    </div>
</div>

    
    


    <script type="text/javascript">
        var isDocumentReady = false;
        var isMapReady = false;

        window.mapReady = function () {
            isMapReady = true;
            initMap();
        }

        window.initMap = function () {
            if (!isDocumentReady || !isMapReady)
                return;

            var mappOfficeContainer = $(".map-office-section");
            if (mappOfficeContainer == null || mappOfficeContainer.length == 0)
                return;

            //set your google maps parameters
            var map_zoom = 3, centerLong = 46.862496, centerLat = 103.846656;

            //var locations = [
            //      ['Causeway Bay, HongKong', 22.2859787, 114.1914919, 5],
            //      ['Palm Desert, CA 92260, USA', 33.6917281, -116.4075854, 4],
            //      ['Stockholm, Sweden', 59.3294, 18.0686, 3],
            //      ['Ho Chi Minh city, Vietnam', 10.7500, 106.6667, 2],
            //      ['Ha Noi, Vietnam', 21.0285, 105.8542, 1]
            //];
            var locations = eval(mappOfficeContainer.attr('data-locations'));
            //google map custom marker icon - .png fallback for IE11
            var is_internetExplorer11 = navigator.userAgent.toLowerCase().indexOf('trident') > -1;
            var marker_url = (is_internetExplorer11) ? '/img/cd-icon-location.png' : '/img/cd-icon-location.svg';

            var icon = {
                url: marker_url,
                //scaledSize: new google.maps.Size(25, 25)
            };

            //define the basic color of your map, plus a value for saturation and brightness
            var main_color = '#2d313f',
                saturation_value = -20,
                brightness_value = 5;

            //we define here the style of the map
            var style = [
                {
                    //set saturation for the labels on the map
                    elementType: "labels",
                    stylers: [
                        { saturation: saturation_value }
                    ]
                },
                {
                    //poi stands for point of interest - don't show these lables on the map
                    featureType: "poi",
                    elementType: "labels",

                },
                {
                    //don't show highways lables on the map
                    featureType: 'road.highway',
                    elementType: 'labels',

                },
                {
                    //don't show local road lables on the map
                    featureType: "road.local",
                    elementType: "labels.icon",

                },
                {
                    //don't show arterial road lables on the map
                    featureType: "road.arterial",
                    elementType: "labels.icon",
                    stylers: [
                        { visibility: "off" }
                    ]
                },
                {
                    //don't show road lables on the map
                    featureType: "road",
                    elementType: "geometry.stroke",
                    stylers: [
                        { visibility: "off" }
                    ]
                },
                //style different elements on the map
                {
                    featureType: "transit",
                    elementType: "geometry.fill",
                    stylers: [
                        { hue: main_color },
                        { visibility: "on" },
                        { lightness: brightness_value },
                        { saturation: saturation_value }
                    ]
                },
                {
                    featureType: "poi",
                    elementType: "geometry.fill",
                    stylers: [
                        { hue: main_color },
                        { visibility: "on" },
                        { lightness: brightness_value },
                        { saturation: saturation_value }
                    ]
                },
                {
                    featureType: "poi.government",
                    elementType: "geometry.fill",
                    stylers: [
                        { hue: main_color },
                        { visibility: "on" },
                        { lightness: brightness_value },
                        { saturation: saturation_value }
                    ]
                },
                {
                    featureType: "poi.sport_complex",
                    elementType: "geometry.fill",
                    stylers: [
                        { hue: main_color },
                        { visibility: "on" },
                        { lightness: brightness_value },
                        { saturation: saturation_value }
                    ]
                },
                {
                    featureType: "poi.attraction",
                    elementType: "geometry.fill",
                    stylers: [
                        { hue: main_color },
                        { visibility: "on" },
                        { lightness: brightness_value },
                        { saturation: saturation_value }
                    ]
                },
                {
                    featureType: "poi.business",
                    elementType: "geometry.fill",
                    stylers: [
                        { hue: main_color },
                        { visibility: "on" },
                        { lightness: brightness_value },
                        { saturation: saturation_value }
                    ]
                },
                {
                    featureType: "transit",
                    elementType: "geometry.fill",
                    stylers: [
                        { hue: main_color },
                        { visibility: "on" },
                        { lightness: brightness_value },
                        { saturation: saturation_value }
                    ]
                },
                {
                    featureType: "transit.station",
                    elementType: "geometry.fill",
                    stylers: [
                        { hue: main_color },
                        { visibility: "on" },
                        { lightness: brightness_value },
                        { saturation: saturation_value }
                    ]
                },
                {
                    featureType: "landscape",
                    stylers: [
                        { hue: main_color },
                        { visibility: "on" },
                        { lightness: brightness_value },
                        { saturation: saturation_value }
                    ]
                },
                {
                    featureType: "road",
                    elementType: "geometry.fill",
                    stylers: [
                        { hue: main_color },
                        { visibility: "on" },
                        { lightness: brightness_value },
                        { saturation: saturation_value }
                    ]
                },
                {
                    featureType: "road.highway",
                    elementType: "geometry.fill",
                    stylers: [
                        { hue: main_color },
                        { visibility: "on" },
                        { lightness: brightness_value },
                        { saturation: saturation_value }
                    ]
                },
                {
                    featureType: "water",
                    elementType: "geometry",
                    stylers: [
                        { hue: main_color },
                        { visibility: "on" },
                        { lightness: brightness_value },
                        { saturation: saturation_value }
                    ]
                }
            ];

            if ($(window).width() < 768) {
                map_zoom = 2;
                if (locations.length > 0) {
                    centerLong = locations[0][1];
                    centerLat = locations[0][2];
                }
            }

            if ($('#google-container').height() > $(window).height() * 0.8) {
                $('#google-container').height($(window).height() * 0.8);
            }

            //set google map options
            var map_options = {
                center: new google.maps.LatLng(centerLong, centerLat),
                zoom: map_zoom,
                panControl: false,
                zoomControl: false,
                mapTypeControl: false,
                streetViewControl: false,
                mapTypeId: google.maps.MapTypeId.ROADMAP,
                scrollwheel: false,
                styles: style,
            };
            //inizialize the map
            var map = new google.maps.Map(document.getElementById('google-container'), map_options);
            //add a custom marker to the map
            var infowindow = new google.maps.InfoWindow();

            var marker, i, currentMark;
            var markers = new Array();

            for (i = 0; i < locations.length; i++) {
                marker = new google.maps.Marker({
                    position: new google.maps.LatLng(locations[i][1], locations[i][2]),
                    map: map,
                    icon: icon,
                    animation: google.maps.Animation.DROP
                });

                google.maps.event.addListener(marker, 'click', (function (marker, i) {
                    return function () {
                        //marker.setAnimation(google.maps.Animation.BOUNCE);
                        infowindow.setContent(locations[i][0]);
                        infowindow.open(map, marker);
                        currentMark = marker;
                    };
                })(marker, i));
                markers.push(marker);
            }
            google.maps.event.addListener(infowindow, 'closeclick', function () {
                currentMark.setAnimation(null);
            });


            google.maps.event.addListenerOnce(map, 'idle', function () {
                var zoomControlDiv = document.createElement('div');
                var zoomControl = new CustomZoomControl(zoomControlDiv, map);

                //insert the zoom div on the top left of the map
                map.controls[google.maps.ControlPosition.LEFT_TOP].push(zoomControlDiv);
            });


            $(".location-item .icon-map").each(function (index) {
                $(this).on('click', function () {
                    $("html, body").animate({
                        scrollTop: '-85px'
                    }, 300);
                    map.setZoom(18);
                    map.setCenter(markers[index].getPosition());
                    google.maps.event.trigger(markers[index], 'click');
                });
            });
        };

        //add custom buttons for the zoom-in/zoom-out on the map
        window.CustomZoomControl = function (controlDiv, map) {
            //grap the zoom elements from the DOM and insert them in the map
            var controlUIzoomIn = document.getElementById('cd-zoom-in'),
                controlUIzoomOut = document.getElementById('cd-zoom-out');
            controlDiv.appendChild(controlUIzoomIn);
            controlDiv.appendChild(controlUIzoomOut);

            // Setup the click event listeners and zoom-in or out according to the clicked element
            google.maps.event.addDomListener(controlUIzoomIn, 'click', function () {
                map.setZoom(map.getZoom() + 1);
            });
            google.maps.event.addDomListener(controlUIzoomOut, 'click', function () {
                map.setZoom(map.getZoom() - 1);
            });
        };
    </script>

    <script src="//maps.google.com/maps/api/js?sensor=false&amp;callback=mapReady"></script>
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/2.1.4/jquery.min.js"></script>
    <script src="http://vjs.zencdn.net/4.12/video.js"></script>

    <script type="text/javascript">
        $(document).ready(function () {
            isDocumentReady = true;
            initMap();
        });

    </script>

        <script src="/js/scripts.js"></script>

</body>



</html>

