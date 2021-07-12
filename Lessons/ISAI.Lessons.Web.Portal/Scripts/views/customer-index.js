$(document).ready(function () {


    class CustomerAdaptor extends ej.data.ODataV4Adaptor {

        processResponse() {

            var original = super.processResponse.apply(this, arguments);
            return original;
        }
    }

    var CustomerDataManager = new ej.data.DataManager({
        url: '/odata/customers',
        adaptor: new CustomerAdaptor(),
        crossDomain: true
    });

    var CustomerGrid = new ej.grids.Grid({
        dataSource: CustomerDataManager,
        editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: 'Dialog', newRowPosition: 'Top' },
        allowPaging: true,
        width: 'auto',
        allowExcelExport: true,
        allowPdfExport: true,
        pageSettings: { pageCount: 4, pageSize: 50 },
        toolbar: [/*'Add'*/ 'Edit', /*'Delete',*/ 'Update', 'Cancel', 'ExcelExport', 'PdfExport', 'CsvExport'],
        actionBegin: function (args) {


            if (args.requestType === "beginEdit" || args.requestType === 'add') {
                this.columns[0].visible = false;


            } else if (args.requestType === "save" || args.requestType === "cancel") {
                this.columns[0].visible = true;
 
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
                field: 'FirstName',
                headerText: 'First Name',
                validationRules: { required: true },
                width: 150

            },


            {
                field: 'LastName',
                headerText: 'Last Name',
                validationRules: { required: true },
                width: 150
            },


            {
                field: 'Email',
                headerText: 'Email',
                validationRules: { required: true },
                width: 150
            },


            {
                field: 'HearAbout',
                headerText: 'Hear About',
                validationRules: { required: true },
                width: 150
            },

            {
                field: 'DateCreated',
                headerText: 'Date Created',
                width: 120,
                editType: 'datetimepickeredit',
                format: { type: 'dateTime', format: 'dd/MM/yyyy HH:mm' }

            },


            {
                field: 'HasCompletedCheckout',
                headerText: 'Checkout Complete',
                displayAsCheckBox: true,
                editType: "booleanedit",
                width: 120

            },


            {
                field: 'AcceptMarketing',
                headerText: 'Accept Marketing',
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