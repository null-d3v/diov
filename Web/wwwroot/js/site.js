(function()
{
    var header = document.getElementById("header");

    var headerHeight = header.offsetHeight;
    var headerY = headerHeight;
    var scrollY = document.body.scrollTop ||
        document.documentElement.scrollTop;

    window.addEventListener("scroll", function()
    {
        var scrollTop = document.body.scrollTop ||
            document.documentElement.scrollTop;
        headerY += scrollY - scrollTop;
        if (headerY > headerHeight)
        {
            headerY = headerHeight;
        }
        else if (headerY < 0)
        {
            headerY = 0;
        }
        header.style.transform =
            "translateY(" + (headerY - headerHeight) + "px)";
        scrollY = scrollTop;
    });
})();