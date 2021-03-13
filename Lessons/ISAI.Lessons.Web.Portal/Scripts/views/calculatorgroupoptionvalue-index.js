var calculatorGroupOptionValueDataManager;
var calculatorGroupOptionValueGrid;

$(document).ready(function () {

    InitDataManagers();
    InitGrid();

});


function InitDataManagers() {

   calculatorGroupOptionValueDataManager = new ej.data.DataManager({
       url: '/odata/calculatorgroupoptionvalues',
        adaptor: new ej.data.ODataV4Adaptor,
        crossDomain: true
   });



}


function InitGrid() {


    calculatorGroupOptionValueGrid = new ej.grids.Grid({
        dataSource: calculatorGroupOptionValueDataManager,
        query: new ej.data.Query().where('calculatorGroupOptionId', 'equal', calculatorGroupOptionId),
        editSettings: { showDeleteConfirmDialog: true, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Dialog', newRowPosition: 'Top' },
        allowPaging: true,
        allowSorting: true,
        sortSettings: { columns: [{ field: 'ListOrder', direction: 'Ascending' }] },
        allowResizing: true,
        allowExcelExport: true,
        allowPdfExport: true,
        allowRowDragAndDrop: true,
        pageSettings: { pageCount: 4, pageSize: 50 },
        toolbar: ['Add', 'Edit', 'Delete', 'Update', 'Cancel'],
        width: 'auto',
        actionBegin: function (args) {

            if (args.requestType === "beginEdit" || args.requestType === 'add') {
                this.columns[0].visible = false;
                this.columns[10].visible = false;
          

            } else if (args.requestType === "save" || args.requestType === "cancel") {
                this.columns[0].visible = true;
                this.columns[10].visible = true;
             
            }

        },
        rowDrop: function (args) {

            var dropIndex = args.dropIndex;
            var rowId = args.data.Id;
            ResortRow(rowId, dropIndex);

        },
        columns: [

            {
                field: 'Id',
                isPrimaryKey: true,
                isIdentity: true,
                allowEditing: false,
                defaultValue: 0,
                width: 75,
                allowSorting: false
            },

            {
                field: 'CalculatorGroupOptionId',
                allowEditing: false,
                visible: false,
                defaultValue: calculatorGroupOptionId,
                allowSorting: false
            },

            {
                field: 'Name',
                headerText: 'Name',
                width: 200,
                allowSorting: false
            },

 

            {
                field: 'IsGroupHeader',
                headerText: 'Text Group Header',
                displayAsCheckBox: true,
                editType: "booleanedit",
                width: 120,
                allowSorting: false

            },

            {
                field: 'ImageUrl',
                headerText: 'Image Url',
                width: 200,
                allowSorting: false
            },


            {
                field: 'Notes',
                headerText: 'Notes',
                width: 200,
                allowSorting: false
            },


            {
                field: 'PriceMaterial',
                headerText: 'Price - Paper',
                validationRules: { required: true },
                defaultValue: 0,
                editType: 'numericedit',
                edit: {
                    params: {
                        validateDecimalOnType: true,
                        decimals: 4,
                        format: 'n4',
                        showSpinButton: false
                    }
                },
                width: 130,
                format: "n4",
                allowSorting: false

            },



            {
                field: 'PriceCopy',
                headerText: 'Price - Copy',
                validationRules: { required: true },
                defaultValue: 0,
                editType: 'numericedit',
                edit: {
                    params: {
                        validateDecimalOnType: true,
                        decimals: 4,
                        format: 'n4',
                        showSpinButton: false
                    }
                },
                width: 130,
                format: "n4",
                allowSorting: false

            },

            {
                field: 'Weight',
                headerText: 'Weight',
                validationRules: { required: true },
                defaultValue: 0,
                editType: 'numericedit',
                edit: {
                    params: {
                        validateDecimalOnType: true,
                        decimals: 4,
                        format: 'n4',
                        showSpinButton: false
                    }
                },
                width: 130,
                format: "n2",
                allowSorting: false

            },


            {
                field: 'WeightRatio',
                headerText: 'Weight Ratio',
                validationRules: { required: true },
                defaultValue: 0,
                editType: 'numericedit',
                edit: {
                    params: {
                        validateDecimalOnType: true,
                        decimals: 4,
                        format: 'n4',
                        showSpinButton: false
                    }
                },
                width: 130,
                format: "n2",
                allowSorting: false

            },


            {
                field: 'ListOrder',
                headerText: 'List Order',
                allowEditing: false,
                allowSorting: false
            },

        ],

        actionComplete: function (args) {

            if (args.requestType === 'delete' || args.requestType === 'save') {
                calculatorGroupOptionValueGrid.refresh();
                return;
            }
         
        }

    });
    calculatorGroupOptionValueGrid.appendTo('#Grid');


}


async function ResortRow(rowId, dropIndex) {


    var url = "/api/calculator/sortgroupopptionvalue?dropIndex=" + dropIndex + "&rowId=" + rowId;

    $.ajax({
        url: url,
        type: "GET",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            calculatorGroupOptionValueGrid.refresh();

        },
        error: function (xhRequest, ErrorText, thrownError) {

            alert(ErrorText);

        }
    });




}
