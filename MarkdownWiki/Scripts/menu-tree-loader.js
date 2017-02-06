function loadMenuTree(url) {
        jQuery.get(url,
            function (data) {
                $('#menuNodes').treeview({
                    data: data, enableLinks: true, emptyIcon: "", showIcon:false
            });
                $('#menuNodes').treeview('collapseAll', { silent: true });
        });
}
