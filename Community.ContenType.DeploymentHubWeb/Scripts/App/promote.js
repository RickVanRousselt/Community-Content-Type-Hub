
$(function () {
    //renders trees

    if (typeof treeData !== "undefined" && treeData !== "") {
        var treeArray = JSON.parse(treeData);

        $("#TreeGrid")
            .jstree({
                "core": {
                    "data": treeArray
                },
                "checkbox": {
                    three_state: false, // to avoid that fact that checking a node also check others
                    cascade: 'down'
                },
                "search": {
                    "case_insensitive": false
                },
                "plugins": ["themes", "json_data", "checkbox", "ui", "search"]
            });

        var to = false;
        $('#searchInput').keyup(function () {
            if (to) { clearTimeout(to); }
            to = setTimeout(function () {
                var v = $('#searchInput').val();
                $('#TreeGrid').jstree(true).search(v);
            }, 250);
        });
        //hookup button events
        var searchBoxElements = document.querySelectorAll(".ms-SearchBox");
        new fabric["SearchBox"](searchBoxElements[0]);

    }



    $(".promoteContentTypeBtn").on("click", PromoteContentTypes);
    $(".CancelButton").on("click", CancelButton_Clicked);
});


var PromoteContentTypes = function (event) {
    var selectedContentTypes = [];

    //build array of all selected items
    var selectedContentTypesInTree = $("#TreeGrid").jstree(false).get_bottom_selected(true);

    for (var s = 0; s < selectedContentTypesInTree.length; s++) {
        selectedContentTypes.push(selectedContentTypesInTree[s].data.ContentTypeId);
    }

    //put array in hidden field
    $("#selectedContentTypeData").val(JSON.stringify(selectedContentTypes));

    //post the form to server
    $("#subscriptionForm").submit();
};

var CancelButton_Clicked = function () {

    window.location = spHostUrl;
    return false;
};


