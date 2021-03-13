var calculatorPriceBandDataManager;
var calculatorPriceBandGrid;

$(document).ready(function () {

    InitDataManagers();
    InitGrid();

});


function InitDataManagers() {

   calculatorPriceBandDataManager = new ej.data.DataManager({
       url: '/odata/calculatorpricebands',
        adaptor: new ej.data.ODataV4Adaptor,
        crossDomain: true
   });

}


function InitGrid() {


    calculatorPriceBandGrid = new ej.grids.Grid({
        dataSource: calculatorPriceBandDataManager,
        query: new ej.data.Query().where('CalculatorId', 'equal', calculatorId),
        editSettings: { showDeleteConfirmDialog: true, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Dialog', newRowPosition: 'Top' },
        allowPaging: true,
        allowSorting: true,
        allowResizing: true,
        allowExcelExport: true,
        allowPdfExport: true,
        pageSettings: { pageCount: 4, pageSize: 50 },
        toolbar: ['Add', 'Edit', 'Delete', 'Update', 'Cancel'],
        width: 'auto',
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
                allowEditing: false,
                defaultValue: 0,
                visible: false,
            },

            {
                field: 'CalculatorId',
                allowEditing: false,
                visible: false,
                defaultValue: calculatorId,
            },

            {
                field: 'QuantityFrom',
                headerText: 'Quantity From',
                validationRules: { required: true },
                defaultValue: 0,
                editType: 'numericedit',
                edit: {
                    params: {
                        validateDecimalOnType: true,
                        decimals: 0,
                        format: 'n0',
                        showSpinButton: false
                    }
                },
                width: 130,
                format: "n0",

            },

            {
                field: 'QuantityTo',
                headerText: 'Quantity To',
                validationRules: { required: true },
                defaultValue: 0,
                editType: 'numericedit',
                edit: {
                    params: {
                        validateDecimalOnType: true,
                        decimals: 0,
                        format: 'n0',
                        showSpinButton: false
                    }
                },
                width: 130,
                format: "n0",

            },


            {
                field: 'PercentageAdjustment',
                headerText: 'Percentage Adjustment',
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
                width: 130,
                format: "n2",

            },

           

        ],

        actionComplete: function (args) {

            if (args.requestType === 'delete') {
                calculatorPriceBandGrid.refresh();
                return;
            }

        }

    });
    calculatorPriceBandGrid.appendTo('#Grid');


}


