
$(function () {
    $(".CancelButton").on("click", CancelButton_Clicked);

    $(".confirmButton").on("click", ConfirmAction);

    var ToggleElements = document.querySelectorAll(".ms-Toggle");
    for (var i = 0; i < ToggleElements.length; i++) {
        new fabric['Toggle'](ToggleElements[i]);
    }

});

var CancelButton_Clicked = function () {
    window.location = spHostUrl;
    return false;
}


var ConfirmAction = function () {

    var override = $("#overrideChecks")[0].checked;
    $("#overrideChecksData").val(override);
    //post the form to server
    $("#subscriptionForm").submit();
    return false;
};