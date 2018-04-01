function ShowErrorMessage(errorMsg) {
    var errorMessage = '<ul class="list-group">'
    errorMessage += errorMsg;
    errorMessage += '</ul>';

    $('#divError').show();
    $('#divError').html(errorMsg);
}

function validateDateFormat(date) {
    var re = /^(0?[1-9]|[12][0-9]|3[01])[\/](0?[1-9]|1[012])[\/]\d{4}$/;
    return re.test(date);
}