$(document).ready(function () {


    class AlloyAdaptor extends ej.data.ODataV4Adaptor {

        processResponse() {

            var original = super.processResponse.apply(this, arguments);
            return original;
        }
    }

    var AppDataManager = new ej.data.DataManager({
        url: '/odata/apps',
        adaptor: new AlloyAdaptor(),
        crossDomain: true
    });

    var AppGrid = new ej.grids.Grid({
        dataSource: AppDataManager,
        editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: 'Dialog', newRowPosition: 'Top' },
        allowPaging: true,
        width: 'auto',
        pageSettings: { pageCount: 4, pageSize: 50 },
        toolbar: [/*'Add'*/ 'Edit', /*'Delete',*/ 'Update', 'Cancel'],
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
                field: 'Name',
                headerText: 'Name',
                validationRules: { required: true },
                width: 200
            },


            {
                field: 'SquareUseSandbox',
                headerText: 'Use Square Sandbox?',
                displayAsCheckBox: true,
                editType: "booleanedit",
                width: 120

            },



            {
                field: 'AllowCollectAtStore',
                headerText: 'Allow Collect At Store?',
                displayAsCheckBox: true,
                editType: "booleanedit",
                width: 120

            },


            {
                field: 'ShippingLoadingIsPercent',
                headerText: 'Pecentage Shipping Loading?',
                displayAsCheckBox: true,
                editType: "booleanedit",
                width: 120

            },



            {
                field: 'ShippingLoading',
                headerText: 'Shipping Loading',
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
                field: 'MinimumOrder',
                headerText: 'Minimum Order',
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

          

        ],
    });
    AppGrid.appendTo('#AppGrid');


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