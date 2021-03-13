$(document).ready(function () {


    class OrderAdaptor extends ej.data.ODataV4Adaptor {

        processResponse() {

            var original = super.processResponse.apply(this, arguments);
            return original;
        }
    }

    var OrderDataManager = new ej.data.DataManager({
        url: '/odata/customerorders',
        adaptor: new OrderAdaptor(),
        crossDomain: true
    });

    var orderStatusDataManager = new ej.data.DataManager({
        url: '/odata/orderstatuses',
        adaptor: new ej.data.ODataV4Adaptor,
        crossDomain: true
    });

    var OrderGrid = new ej.grids.Grid({
        dataSource: OrderDataManager,
        editSettings: { showDeleteConfirmDialog: true, allowEditing: false, allowAdding: false, allowDeleting: true, mode: 'Dialog', newRowPosition: 'Top' },
        allowPaging: true,
        allowSorting: true,
        sortSettings: { columns: [{ field: 'Id', direction: 'Descending' }] },
        width: 'auto',
        pageSettings: { pageCount: 4, pageSize: 50 },
        toolbar: ['Delete', 'Cancel'],

        recordDoubleClick: function (args) {
            document.body.style.cursor = 'wait';
            document.location.href = '/order/edit/' + args.rowData.Id;
        },
        columns: [


            {
                field: 'Id',
                isPrimaryKey: true,
                isIdentity: true,
                headerText: 'Id',
                allowEditing: false,
                defaultValue: 0,
                width: 70,
            },


            {
                field: 'Id',
                headerText: 'Order Date',
                width: 150,
                valueAccessor: function (field, data, column) {

                    return luxon.DateTime.fromJSDate(data.OrderDate).setZone('America/New_York').toLocaleString(luxon.DateTime.DATETIME_MED);

                }
            },

            {
                field: 'SubTotalCost',
                headerText: 'Sub Total',
                validationRules: { required: true },
                defaultValue: 0,
                editType: 'numericedit',
                edit: {
                    params: {
                        validateDecimalOnType: true,
                        decimals: 2,
                        format: 'n2',
                        showSpinButton: false
                    }
                },
                width: 100,
                format: "n2",

            },

            {
                field: 'ShippingCost',
                headerText: 'Shipping',
                validationRules: { required: true },
                defaultValue: 0,
                editType: 'numericedit',
                edit: {
                    params: {
                        validateDecimalOnType: true,
                        decimals: 2,
                        format: 'n2',
                        showSpinButton: false
                    }
                },
                width: 100,
                format: "n2",

            },


            {
                field: 'TaxCost',
                headerText: 'Tax',
                validationRules: { required: true },
                defaultValue: 0,
                editType: 'numericedit',
                edit: {
                    params: {
                        validateDecimalOnType: true,
                        decimals: 2,
                        format: 'n2',
                        showSpinButton: false
                    }
                },
                width: 100,
                format: "n2",

            },

            {
                field: 'TotalCost',
                headerText: 'Total',
                validationRules: { required: true },
                defaultValue: 0,
                editType: 'numericedit',
                edit: {
                    params: {
                        validateDecimalOnType: true,
                        decimals: 2,
                        format: 'n2',
                        showSpinButton: false
                    }
                },
                width: 100,
                format: "n2",

            },

            {
                field: 'TotalWeight',
                headerText: 'Total Weight',
                validationRules: { required: true },
                defaultValue: 0,
                editType: 'numericedit',
                edit: {
                    params: {
                        validateDecimalOnType: true,
                        decimals: 2,
                        format: 'n2',
                        showSpinButton: false
                    }
                },
                width: 100,
                format: "n2",

            },


            {
                field: 'OrderStatusId',
                foreignKeyField: 'Id',
                foreignKeyValue: 'Name',
                dataSource: orderStatusDataManager,
                width: 130,
                headerText: 'Order Status',
                validationRules: { required: true }


            }

            
          

        ],
    });
    OrderGrid.appendTo('#Grid');


    function actionBegin(args, target) {
        if (args.requestType === 'save') {
            if (target.pageSettings.currentPage !== 1 && target.editSettings.newRowPosition === 'Top') {
                args.index = (target.pageSettings.currentPage * target.pageSettings.pageSize) - target.pageSettings.pageSize;
            } else if (target.editSettings.newRowPosition === 'Bottom') {
                args.index = (target.pageSettings.currentPage * target.pageSettings.pageSize) - 1;
            }
        }
    }

});