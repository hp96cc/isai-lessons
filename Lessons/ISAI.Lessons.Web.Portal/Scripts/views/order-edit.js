var customerOrder;
var customerCart;

var orderStatusDataManager;

var editOrderStatusId;
var editSubTotalCost;
var editTaxCost;
var editShippingCost;
var editTotalCost;
var editTotalWeight;

var editInvoiceAddressLine1;
var editInvoiceAddressLine2;
var editInvoiceAddressLine3;
var editInvoiceAddressCity;
var editInvoiceAddressState;
var editInvoiceAddressZipOrPostcode;
var editInvoiceAddressCountry;


var editDeliveryAddressLine1;
var editDeliveryAddressLine2;
var editDeliveryAddressLine3;
var editDeliveryAddressCity;
var editDeliveryAddressState;
var editDeliveryAddressZipOrPostcode;
var editDeliveryAddressCountry;

var editEstimatedDeliveryDate;
var editUPSTrackingCode;

var selectedOrderStatusId;

$(document).ready(function () {


    InitDataManagers();
    InitPageControls();

    LoadData();

});

function LoadData() {

    var url = "/odata/customerorders(" + customerOrderId + ")?$expand=Customer,OrderStatus,InvoiceAddress,DeliveryAddress";
    $('.ibox-content').toggleClass('sk-loading');

    $.ajax({
        url: url,
        type: "GET",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            customerOrder = data;
            customerCart = JSON.parse(data.CartJson);
            DisplayData();

            $('.ibox-content').toggleClass('sk-loading');

        },
        error: function (xhRequest, ErrorText, thrownError) {

            console.log(ErrorText);
            alert("Error: Could not load data. See browser log for details.");
            $('.ibox-content').toggleClass('sk-loading');

        }
    });

}



function DisplayData() {

    var formattedOrderDate = luxon.DateTime.fromISO(customerOrder.OrderDate).setZone('America/New_York').toLocaleString(luxon.DateTime.DATE_MED_WITH_WEEKDAY);

    $('#CustomerOrderId').text(customerOrder.Id);
    $('#CustomerName').text(customerOrder.Customer.FirstName + ' ' + customerOrder.Customer.LastName);
    $('#OrderDate').text(formattedOrderDate);

    $('#PaymentId').text(customerCart.PaymentId);
    $('#Nonce').text(customerCart.Nonce);

    editOrderStatusId.value = customerOrder.OrderStatusId === null ? 'No Status' : customerOrder.OrderStatus.Name;

    editDeliveryAddressLine1.value = customerOrder.DeliveryAddress.Line1 ? customerOrder.InvoiceAddress.Line1 : ' ';
    editDeliveryAddressLine2.value = customerOrder.DeliveryAddress.Line2 ? customerOrder.DeliveryAddress.Line2 : ' ';
    editDeliveryAddressCity.value = customerOrder.DeliveryAddress.City ? customerOrder.DeliveryAddress.City : ' ';
    editDeliveryAddressState.value = customerOrder.DeliveryAddress.State ? customerOrder.DeliveryAddress.State : ' ';
    editDeliveryAddressZipOrPostcode.value = customerOrder.DeliveryAddress.Postcode ? customerOrder.DeliveryAddress.Postcode : ' ';

    editInvoiceAddressLine1.value = customerOrder.InvoiceAddress.Line1 ? customerOrder.InvoiceAddress.Line1  : ' ';
    editInvoiceAddressLine2.value = customerOrder.InvoiceAddress.Line2 ? customerOrder.InvoiceAddress.Line2 : ' ' ;
    editInvoiceAddressCity.value = customerOrder.InvoiceAddress.City ? customerOrder.InvoiceAddress.City : ' ';
    editInvoiceAddressState.value = customerOrder.InvoiceAddress.State ? customerOrder.InvoiceAddress.State : ' ';
    editInvoiceAddressZipOrPostcode.value = customerOrder.InvoiceAddress.Postcode ? customerOrder.InvoiceAddress.Postcode : ' ';

    editUPSTrackingCode.value = customerOrder.UPSTrackingCode;
    editEstimatedDeliveryDate.value = customerOrder.EstimatedDeliveryDate;

    UpdateTotals();
}

async function PatchData(propertyName, propertyValue) {

    var url = "/odata/customerorders(" + customerOrder.Id + ")";
    var payload = {
        Id: customerOrderId.Id
    };

    payload[propertyName] = propertyValue;

    try {

        var data = await $.ajax({
            url: url,
            type: "PATCH",
            data: JSON.stringify(payload),
            contentType: 'application/json'

        });

        console.log(data);

    } catch (error) {

        console.log(error);
        alert("Error: Could not create record. See browser log for details.");
    }

}

function InitDataManagers() {

    orderStatusDataManager = new ej.data.DataManager({
        url: '/odata/orderstatuses',
        adaptor: new ej.data.ODataV4Adaptor,
        crossDomain: true
    });
}

function InitPageControls() {

    editOrderStatusId = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Popup',
        type: 'DropDownList',
        actionOnBlur: 'Cancel',
        model: {
            dataSource: orderStatusDataManager,
            placeholder: "Select Order Status",
            fields: { text: 'Name', value: 'Id' },
            width: 400,
            change: function (args) {
               selectedOrderStatusId = args.value;
            }
        },
        actionSuccess: async function (ActionEventArgs) {

            if (selectedOrderStatusId) {
                customerOrder.OrderStatusId = selectedOrderStatusId;
                await PatchData("OrderStatusId", customerOrder.OrderStatusId);
                LoadData();
            }
        },

    });
    editOrderStatusId.appendTo('#OrderStatus');


    editUPSTrackingCode = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Text',
        model: {
            placeholder: 'UPS Tracking Code'
        },
        actionSuccess: async function (ActionEventArgs) {
            customerOrder.UPSTrackingCode = ActionEventArgs.value;
            await PatchData("UPSTrackingCode", customerOrder.UPSTrackingCode);
        }
    });

    editUPSTrackingCode.appendTo('#UPSTrackingCode');



    editEstimatedDeliveryDate = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Date',
        name: 'editDateIn',
        model: {
            placeholder: 'Select a date',
        },
        validationRules: {
            editDateIn: { required: true }
        },
        validating: function (e) {
            e.errorMessage = 'Required';
        },
        actionSuccess: async function (ActionEventArgs) {
            customerOrder.EstimatedDeliveryDate = new Date(ActionEventArgs.value).toISOString();
            await PatchData("EstimatedDeliveryDate", customerOrder.EstimatedDeliveryDate);
        }
    });
    editEstimatedDeliveryDate.appendTo('#EstimatedDeliveryDate');

    editSubTotalCost = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Text',
        disabled: true
    });

    editSubTotalCost.appendTo('#SubTotalCost');

    editTaxCost = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Text',
        disabled: true
    });

    editTaxCost.appendTo('#TaxCost');





    editShippingCost = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Text',
        disabled: true
    });

    editShippingCost.appendTo('#ShippingCost');

    editTotalCost = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Text',
        disabled: true
    });

    editTotalCost.appendTo('#TotalCost');


    editTotalWeight = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Text',
        disabled: true
    });

    editTotalWeight.appendTo('#TotalWeight');


    editDeliveryAddressLine1 = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Text',
        disabled: true
    });

    editDeliveryAddressLine1.appendTo('#DeliveryAddressLine1');

    editDeliveryAddressLine2 = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Text',
        disabled: true
    });

    editDeliveryAddressLine2.appendTo('#DeliveryAddressLine2');

    editDeliveryAddressCity = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Text',
        disabled: true
    });

    editDeliveryAddressCity.appendTo('#DeliveryAddressCity');

    editDeliveryAddressState = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Text',
        disabled: true
    });

    editDeliveryAddressState.appendTo('#DeliveryAddressState');

    editDeliveryAddressZipOrPostcode = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Text',
        disabled: true
    });

    editDeliveryAddressZipOrPostcode.appendTo('#DeliveryAddressZipOrPostcode');

    editInvoiceAddressLine1 = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Text',
        disabled: true
    });

    editInvoiceAddressLine1.appendTo('#InvoiceAddressLine1');

    editInvoiceAddressLine2 = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Text',
        disabled: true
    });

    editInvoiceAddressLine2.appendTo('#InvoiceAddressLine2');

    editInvoiceAddressCity = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Text',
        disabled: true
    });

    editInvoiceAddressCity.appendTo('#InvoiceAddressCity');

    editInvoiceAddressState = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Text',
        disabled: true
    });

    editInvoiceAddressState.appendTo('#InvoiceAddressState');

    editInvoiceAddressZipOrPostcode = new ej.inplaceeditor.InPlaceEditor({
        mode: 'Inline',
        type: 'Text',
        disabled: true
    });

    editInvoiceAddressZipOrPostcode.appendTo('#InvoiceAddressZipOrPostcode');



}


function UpdateTotals() {

    const options = {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    };

    editSubTotalCost.value = 'USD ' + Number(customerOrder.SubTotalCost).toLocaleString('en', options);
    editTaxCost.value = 'USD ' + Number(customerOrder.TaxCost).toLocaleString('en', options)
    editShippingCost.value = 'USD ' + Number(customerOrder.ShippingCost).toLocaleString('en', options);
    editTotalCost.value = 'USD ' + Number(customerOrder.TotalCost).toLocaleString('en', options);
    editTotalWeight.value = Number(customerOrder.TotalWeight).toLocaleString('en', options);


}

async function SendCustomerOrderDispatchedEmail() {

    $('.ibox-content').toggleClass('sk-loading');
    var url = "/api/order/customerorderdispatchedemail?orderId=" + customerOrderId;

    try {

        var data = await $.ajax({
            url: url,
            type: "GET",
            contentType: 'application/json'

        });

        console.log(data);

    } catch (error) {

        console.log(error);
        alert("Error: Could not create record. See browser log for details.");
    }

    $('.ibox-content').toggleClass('sk-loading');
}


async  function SendCustomerOrderEmail() {

    $('.ibox-content').toggleClass('sk-loading');
    var url = "/api/order/customerorderemail?orderId=" + customerOrderId;

    try {

        var data = await $.ajax({
            url: url,
            type: "GET",
            contentType: 'application/json'

        });

        console.log(data);

    } catch (error) {

        console.log(error);
        alert("Error: Could not create record. See browser log for details.");
    }

    $('.ibox-content').toggleClass('sk-loading');

}