function validateDetails() {

    var boardSize = $('#ddlBoardSize').val() || '';
    var propertyState = $('#ddlState').val() || '';
    var fromDate = $('#txtFromDate').val() || '';
    var toDate = $('#txtToDate').val() || '';
    var errorMessage = '';

    if (boardSize == '') {
        errorMessage += '<li class="list-group-item list-group-item-danger">Please select board size.</li>';
    }
    if (propertyState == '') {
        errorMessage += '<li class="list-group-item list-group-item-danger">Please select property state.</li>';
    }

    if (validateDateFormat(fromDate)) {
        if (fromDate == '') {
            errorMessage += '<li class="list-group-item list-group-item-danger">Please select date from.</li>';
        }
    }
    else {
        errorMessage += '<li class="list-group-item list-group-item-danger">From Date: Date format must be dd/MM/yyyy</li>';
    }

    if (validateDateFormat(toDate)) {
        if (toDate == '') {
            errorMessage += '<li class="list-group-item list-group-item-danger">Please select date To.</li>';
        }
    }
    else {
        errorMessage += '<li class="list-group-item list-group-item-danger">To Date: Date format must be dd/MM/yyyy</li>';
    }

    if (errorMessage != '') {
        ShowErrorMessage(errorMessage);
        return false;
    }
    return true;
}

function clearAll() {
    $('#ddlBoardSize').val('');
    $('#ddlState').val('');
    $('#txtFromDate').val('');
    $('#txtToDate').val('');

    $('.trCostEstimate').hide();
    $('#divError').hide();
}
