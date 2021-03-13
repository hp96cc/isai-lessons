
var locationScanState = 0;
var locationScanData = [null, null, null];

var _consumableScanCallback;

$(document).ready(function () {


    $(document).bind('keydown', 'f8', function () {
        SetLocationScan(0);
    });

    $(document).bind('keydown', 'f9', function () {
        SetOrderLineScan();
    });


    $(document).bind('keydown', 'f10', function () {
        SetConsumableCheckScan();
    });

    $('#locationScanModal').on('shown.bs.modal', function (e) {
        $("#scan-location-value").focus();
    });

    $('#scan-location-value').keydown(function (e) {
        var keyCode = e.keyCode || e.which;

        if (keyCode === 13) {
            UpdateLocationScanState();
            console.log($('#scan-location-value').val());
            return false;
        }
    });


    $('#orderLineScanModal').on('shown.bs.modal', function (e) {
        $("#scan-orderline-value").focus();
    });

    $('#scan-orderline-value').keydown(function (e) {
        var keyCode = e.keyCode || e.which;

        if (keyCode === 13) {
            UpdateOrderLineScanState();
            console.log($('#scan-orderline-value').val());
            return false;
        }
    });


    $('#consumableScanModal').on('shown.bs.modal', function (e) {
        $("#scan-consumable-value").focus();
    });

    $('#scan-consumable-value').keydown(function (e) {
        var keyCode = e.keyCode || e.which;

        if (keyCode === 13) {
            UpdateConsumableScanState();
            console.log($('#scan-consumable-value').val());
            return false;
        }
    });


    $('#consumableCheckScanModal').on('shown.bs.modal', function (e) {
        $("#scan-consumablecheck-value").focus();
    });

    

    $('#scan-dailychecklist-value').keydown(function (e) {
        var keyCode = e.keyCode || e.which;

        if (keyCode === 13) {
            UpdateDailyChecklistScanState();
            console.log($('#scan-dailychecklist-value').val());
            return false;
        }
    });


    $('#dailyCheckListScanModal').on('shown.bs.modal', function (e) {
        $("#scan-consumablecheck-value").focus();
    });

});


async function CheckScanForExceptions(orderLineId, processId) {

    var url = "/api/orderlinetask/checkscanexceptions?orderLineId=" + orderLineId + "&processId=" + processId;

    try {

        var data = await $.ajax({
            url: url,
            type: "GET",
            contentType: 'application/x-www-form-urlencoded'
        });

    } catch (error) {

        console.log(error);
        SetLocationScanError('Error: Invalid Scan');

    }

    return data;

}

async function UpdateLocationScanState() {


    scanValue = $('#scan-location-value').val();

    if (locationScanState === 2) {

        if (scanValue.toUpperCase().substring(0, 1) === 'P') {

            var processId = parseInt(scanValue.substring(1).trim());  //P0000001026
            locationScanData[locationScanState] = processId;

            console.log(locationScanData[0]);
            console.log(locationScanData[1]);
            console.log(locationScanData[2]);

            var moveExceptions = await CheckScanForExceptions(locationScanData[0], locationScanData[2]);
            var moveExceptionReason = ($("#scan-location-reason").val() + '').trim();

            if (moveExceptionReason.length == 0 && moveExceptions.length > 0) {

                var exceptionText = '';

                for (var i = 0; i < moveExceptions.length; i++) {
                    exceptionText += '<li>' + moveExceptions[i] + "</li>";
                }
                $("#scan-location-exceptions").html(exceptionText);

                $("#scan-location-value").hide();
                $("#scan-location-exception-div").show();
     
                return;
            }

            var url = "/api/orderlinetask/scan"

            var data = {

                OrderLineId: locationScanData[0],
                UserScanId: locationScanData[1],
                ProcessId: locationScanData[2],
                ExceptionReason: moveExceptionReason
            };

            $.ajax({
                url: url,
                type: "POST",
                data: data,
                contentType: 'application/x-www-form-urlencoded',
                success: function (data) {

                    //Perform update, show soinner close when done
                    locationScanState = 0;
                    $('#locationScanModal').modal('hide');

                },
                error: function (xhRequest, ErrorText, thrownError) {

                    SetLocationScanError('Error: ' + ErrorText);

                }
            });



            return;

        } else {

            SetLocationScanError('Not a valid Process');
            return;
        }




    } else if (locationScanState === 0) {


        if (scanValue.toUpperCase().substring(0, 1) === 'L') {

            var orderLineId = parseInt(scanValue.substring(1).trim());  //L0000001026
            locationScanData[locationScanState] = orderLineId;

        } else {

            SetLocationScanError('Not a valid Order Line');
            return;
        }



    } else if (locationScanState === 1) {


        if (scanValue.toUpperCase().substring(0, 1) === 'U') {

            var userId = parseInt(scanValue.substring(1).trim());  //U0000001026
            locationScanData[locationScanState] = userId;

        } else {

            SetLocationScanError('Not a valid User');
            return;
        }

    }


    locationScanState++;
    SetLocationScan(locationScanState);

}


function UpdateOrderLineScanState() {


    scanValue = $('#scan-orderline-value').val();


    if (scanValue.toUpperCase().substring(0, 1) === 'L') {

        var orderLineId = parseInt(scanValue.substring(1).trim());  //++L0000001026
        location.replace('/orderline/edit/' + orderLineId);

        $('#orderLineScanModal').modal('hide');

    } else {

        SetOrderLineScanError('Not a valid Order Line');
        return;
    }


}


function UpdateConsumableScanState() {


    scanValue = $('#scan-consumable-value').val();

    if (scanValue.toUpperCase().substring(0, 1) === 'C') {

        var consumableId = parseInt(scanValue.substring(1).trim());  //++C0000001026
        _consumableScanCallback(consumableId, SetConsumableScanError, CloseConsumableScan);

    } else {

        SetConsumableScanError('Not a valid Consumable');
        return;
    }


}





function UpdateConsumableCheckScanState() {


    scanValue = $('#scan-consumablecheck-value').val();

    if (scanValue.toUpperCase().substring(0, 1) === 'C') {

        var consumableLogId = parseInt(scanValue.substring(1).trim());  

        var url = "/api/consumablelog?consumableLogId=" + consumableLogId;

        $.ajax({
            url: url,
            type: "GET",
            contentType: 'application/x-www-form-urlencoded',
            success: function (data) {

                $('#scan-consumablecheck-result').html("Name: " + data.Consumable.Name + " <br /> Expiry Date: " + new Date(data.ExpiryDate).toDateString());
                $('#scan-consumablecheck-result').show();
                $('#scan-consumable-error').hide();

            },
            error: function (xhRequest, ErrorText, thrownError) {

                console.log(ErrorText);
                alert(ErrorText);

            }
        });

    } else {

        SetConsumableScanCheckError('Not a valid Consumable');
        return;
    }


}

function SetLocationScanError(message) {

    $("#scan-location-value").val('');
    $('#scan-location-error').text(message);
    $('#scan-location-error').show();

}



function SetOrderLineScanError(message) {

    $("#scan-orderline-value").val('');
    $('#scan-orderline-error').text(message);
    $('#scan-orderline-error').show();

}


function SetConsumableScanError(message) {

    $("#scan-consumable-value").val('');
    $('#scan-consumable-error').html(message);
    $('#scan-consumable-error').show();

}

function SetConsumableScanCheckError(message) {

    $("#scan-consumablecheck-value").val('');
    $('#scan-consumablecheck-error').html(message);
    $('#scan-consumablecheck-error').show();
    $('#scan-consumablecheck-result').hide();
    

}



function SetLocationScan(scanState) {


    locationScanState = scanState;

    $('#scan-location-error').text('');
    $('#scan-location-error').hide();
    $("#scan-location-value").val('');
    $("#scan-location-value").show();

    if (locationScanState === 0) {

        locationScanData = [null, null, null];

        $("#scan-location-exception-div").hide();
        $("#scan-location-reason").val('');

        $('#scan-location-label').text("Scan Order Line");
        $('#locationScanModal').modal('show');
    

    } else if (locationScanState === 1) {

        $('#scan-location-label').text("Scan User");

    } else if (locationScanState === 2) {


        $('#scan-location-label').text("Scan Process");

    } else {

        locationScanState = 0;
        $('#locationScanModal').modal('hide');

    }

}




function SetOrderLineScan() {

    $('#scan-orderline-error').text('');
    $('#scan-orderline-error').hide();
    $("#scan-orderline-value").val('');

    $('#orderLineScanModal').modal('show');

}


function SetConsumableScan(callback, consumableGroup) {

    _consumableScanCallback = callback;

    $('#scan-consumable-name').text(consumableGroup.Name);
    $('#scan-consumable-error').text('');
    $('#scan-consumable-error').hide();
    $("#scan-consumable-value").val('');

    $('#consumableScanModal').modal('show');

}

function SetConsumableCheckScan() {

    $('#scan-consumablecheck-error').text('');
    $('#scan-consumablecheck-error').hide();
    $("#scan-consumablecheck-value").val('');
    $('#scan-consumablecheck-result').hide();

    $('#consumableCheckScanModal').modal('show');

}



function CloseConsumableScan() {

    $('#consumableScanModal').modal('hide');

}

