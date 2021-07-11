$(document).ready(function () {


    class CustomerAdaptor extends ej.data.ODataV4Adaptor {

        processResponse() {

            var original = super.processResponse.apply(this, arguments);
            return original;
        }
    }

    var CustomerDataManager = new ej.data.DataManager({
        url: '/odata/subscriptions',
        adaptor: new CustomerAdaptor(),
        crossDomain: true
    });

    var CustomerGrid = new ej.grids.Grid({
        dataSource: CustomerDataManager,
        editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: 'Dialog', newRowPosition: 'Top' },
        query: new ej.data.Query().expand("Customer,SubscriptionType"),
        allowPaging: true,
        width: 'auto',
        allowExcelExport: true,
        allowPdfExport: true,
        allowGrouping: true,
        groupSettings: { disablePageWiseAggregates: true },
        sortSettings: { columns: [{ field: 'StartDate', direction: 'Descending' }] },
        filterSettings: {
            type: 'Excel'
        },
        pageSettings: { pageCount: 4, pageSize: 50 },
        toolbar: [/*'Add'*/ 'Edit', /*'Delete',*/ 'Update', 'Cancel', 'ExcelExport', 'PdfExport', 'CsvExport'],
        actionBegin: function (args) {


            if (args.requestType === "beginEdit" || args.requestType === 'add') {
                this.columns[0].visible = false;
                this.columns[1].visible = false;
                this.columns[2].visible = false;
                this.columns[3].visible = false;
                this.columns[4].visible = false;



            } else if (args.requestType === "save" || args.requestType === "cancel") {
                this.columns[0].visible = true;
                this.columns[1].visible = true;
                this.columns[2].visible = true;
                this.columns[3].visible = true;
                this.columns[4].visible = true;

            }
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
                field: 'Customer.FirstName',
                headerText: 'Customer',
                allowEditing: false,
                width: 150,
                valueAccessor: function (field, data, column) {

                    return data.Customer.FirstName + ' ' + data.Customer.LastName;

                }
            },


            {
                field: 'Name',
                headerText: 'Name',
                allowEditing: false,
                width: 150

            },

            {
                field: 'SubscriptionType.Name',
                headerText: 'Subscription Type',
                allowEditing: false,
                width: 120

            },



            

            {
                field: 'StripeSubscriptionId',
                headerText: 'Stripe Sub Id',
                allowEditing: false,
                width: 150
            },

            {
                field: 'StartDate',
                headerText: 'Start Date',
                width: 120,
                editType: 'datetimepickeredit',
                format: { type: 'dateTime', format: 'dd/MM/yyyy HH:mm' }

            },

            {
                field: 'EndDate',
                headerText: 'End Date',
                width: 120,
                editType: 'datetimepickeredit',
                format: { type: 'dateTime', format: 'dd/MM/yyyy HH:mm' }

            },


            {
                field: 'Active',
                headerText: 'Active',
                displayAsCheckBox: true,
                editType: "booleanedit",
                width: 120

            },


           



        ],

        toolbarClick: async function (args) {


            if (args.item.id === 'Grid_pdfexport') {
                var exportProperties = {
                    pageOrientation: 'Landscape',
                    pageSize: 'A3',
                    isAutoFit: true
                };
                CustomerGrid.pdfExport(exportProperties);
            }

            if (args.item.id === 'Grid_excelexport') {
                CustomerGrid.excelExport();
            }

            if (args.item.id === 'Grid_csvexport') {
                CustomerGrid.csvExport();
            }

        }
    });
    CustomerGrid.appendTo('#Grid');


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