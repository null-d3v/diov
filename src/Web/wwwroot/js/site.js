(() =>
{
    const header = document.getElementById("header");

    const headerHeight = header.offsetHeight;
    let headerY = headerHeight;
    let scrollY = document.body.scrollTop ||
        document.documentElement.scrollTop;

    window.addEventListener("scroll", () =>
    {
        let scrollTop = document.body.scrollTop ||
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