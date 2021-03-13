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
                field: 'CompanyName',
                headerText: 'Company Name',
                width: 150
            },


            {
                field: 'Telephone',
                headerText: 'Telephone',

                width: 150
            },

            {
                field: 'Mobile',
                headerText: 'Mobile',

                width: 150
            },

            {
                field: 'Email',
                headerText: 'Email',
                validationRules: { required: true },
                width: 150
            },


            {
                field: 'AcceptMarketing',
                headerText: 'Accept Marketing',
                displayAsCheckBox: true,
                editType: "booleanedit",
                width: 120

            },


          

        ],
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