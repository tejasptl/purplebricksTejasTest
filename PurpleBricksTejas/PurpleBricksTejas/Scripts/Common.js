function ShowErrorMessage(errorMsg) {
    var errorMessage = '<ul class="list-group">'
    errorMessage += errorMsg;
    errorMessage += '</ul>';

    $('#divError').show();
    $('#divError').html(errorMsg);
}