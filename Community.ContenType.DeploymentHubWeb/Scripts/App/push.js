
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
                "plugins": ["themes", "json_data", "checkbox", "ui", "search"]

            });

        var to = false;
        $('#searchInput')
            .keyup(function() {
                if (to) {
                    clearTimeout(to);
                }
                to = setTimeout(function() {
                        var v = $('#searchInput').val();
                        $('#TreeGrid').jstree(true).search(v);
                    },
                    250);
            });


        //hookup button events
        var searchBoxElements = document.querySelectorAll(".ms-SearchBox");
        new fabric["SearchBox"](searchBoxElements[0]);
        var SpinnerElements = document.querySelectorAll(".ms-Spinner");
        new fabric['Spinner'](SpinnerElements[0]);


    }
    $(".pushContentTypeBtn").on("click", PushContentTypes);
    $(".CancelButton").on("click", CancelButton_Clicked);
    $("#GetSiteCollButton").on("click", GetSitCollbutton_Clicked);

});

var PushContentTypes = function (event) {
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

var DeploymentGroupObject = function () {
    this.Name = "";
    this.ID = "";
}


var GetSitCollbutton_Clicked = function () {

    var selectedDeploymentGroups = [];
    $('.siteCollSpinner').show();
    var myTableDiv = document.getElementById("siteCollectionsTable");
    myTableDiv.innerHTML = "";
    
    //build array of all selected items
    var selectedDeploymentGroupsInTree = $("#TreeGrid").jstree(false).get_top_selected(true);

    if ((selectedDeploymentGroupsInTree.length !== 1) || ($('#TreeGrid').jstree(true).get_parent(selectedDeploymentGroupsInTree[0]) != "#")) {
        alert("Please select one deploymentgroup");
    } else {
        var deploymentGroup = new DeploymentGroupObject();
        deploymentGroup.ID = selectedDeploymentGroupsInTree[0].id;
        deploymentGroup.Name = selectedDeploymentGroupsInTree[0].text;

        var urlAction = GetSiteCollections;

        var deploymentGroupsjson = JSON.stringify(deploymentGroup);

        //send request to server
        $.ajax({
            type: 'POST',
            url: urlAction,
            data: deploymentGroupsjson,
            contentType: "application/json",
            success: function (returnData) {
                if (returnData === null || returnData === "ERROR") {
                    $('.siteCollSpinner').hide();
                    console.error("A error occured in ajax success handler");
                    alert("We could not retrieve the site collections");
                } else {
                    console.log("Get Site Collections Succeeded");
                    var siteCollectionsRecieved = JSON.parse(returnData);
                    $('.siteCollSpinner').hide();

                    var myTableDiv = document.getElementById("siteCollectionsTable");
                    myTableDiv.innerHTML = "";
                    var tableBody = document.createElement('TBODY');

                    for (i = 0; i < siteCollectionsRecieved.length; i++) {
                        var tr = document.createElement('TR');
                        var td = document.createElement('TD');

                        td.appendChild(document.createTextNode(siteCollectionsRecieved[i].Url));
                        tr.appendChild(td);

                        tableBody.appendChild(tr);
                    }
                    myTableDiv.appendChild(tableBody);
                };
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.error("A error occured in ajax error handler -> " + textStatus + " -> " + errorThrown);

            }
        });
    }
    return false;
};