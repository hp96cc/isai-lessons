$(document).ready(function () {



    var AppDataManager = new ej.data.DataManager({
        url: '/odata/vouchers',
        adaptor: new ej.data.ODataV4Adaptor(),
        crossDomain: true
    });

    var AppGrid = new ej.grids.Grid({
        dataSource: AppDataManager,
        editSettings: { allowEditing: true, allowAdding: true, allowDeleting: false, mode: 'Dialog', newRowPosition: 'Top' },
        allowPaging: true,
        width: 'auto',
        pageSettings: { pageCount: 4, pageSize: 50 },
        toolbar: ['Add', 'Edit', 'Delete', 'Update', 'Cancel'],
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
                field: 'Code',
                headerText: 'Code',
                validationRules: { required: true },
                width: 200,
                defaultValue: uuidv4()
            },




            {
                field: 'Amount',
                headerText: 'Amount',
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
                field: 'DateUsed', headerText: 'Date Used', editType: 'datepickeredit',
                format: { type: 'dateTime', format: 'dd/MM/yyyy' },
                width: 125

            },

        ],
    });
    AppGrid.appendTo('#Grid');


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