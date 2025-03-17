var userDataManager;
var userGrid;

var tutorialSubjectTutorUserElement;
var tutorialSubjectTutorUserList;

var rolesDataManager;
var roleDropDown;
var roleDropDownElement;

var tutorialSubjectDataManager;
var tutorialsubjecttutorusersDataManager;

$(document).ready(function () {

    InitDataManagers();
    InituserGrid();
    InitPageControls();

});


function InitDataManagers() {

    userDataManager = new ej.data.DataManager({
        url: '/odata/users',
        adaptor: new UserDataManagerAdaptor,
        crossDomain: true
    });

    rolesDataManager = new ej.data.DataManager({
        url: '/odata/roles',
        adaptor: new ej.data.ODataV4Adaptor,
        crossDomain: true
    });

    tutorialSubjectDataManager = new ej.data.DataManager({
        url: '/odata/tutorialsubjects',
        adaptor: new ej.data.ODataV4Adaptor,
        crossDomain: true
    });

    tutorialsubjecttutorusersDataManager = new ej.data.DataManager({
        url: '/odata/tutorialsubjecttutorusers',
        adaptor: new ej.data.ODataV4Adaptor,
        crossDomain: true
    });


}

class FactoryUserDataManagerAdaptor extends ej.data.ODataV4Adaptor {

    processQuery() {

        var query = super.processQuery.apply(this, arguments);

        var url = query.url.replace(" eq ", " eq '") + "'";

        return {
            type: "GET",
            url: url,
            ejPvtData: this.pvt
        };

    }

}

class UserDataManagerAdaptor extends ej.data.ODataV4Adaptor {

    update(dm, keyField, value, tableName) {

        var userPatch = {};
        userPatch.Fullname = value.Firstname + ' ' + value.Surname;
        userPatch.Firstname = value.Firstname;
        userPatch.Surname = value.Surname;
        userPatch.AllowedTutorialSubjectTutorUserJson = value.TutorialSubjectTutorUsers;
        userPatch.Role = value.Role;
        userPatch.Email = value.Email;
        userPatch.Position = value.Position;
        userPatch.TutorStripeId = value.TutorStripeId;
        userPatch.TutorEmail = value.TutorEmail;

        return {
            type: 'PATCH',
            url: "/odata/users('" + value.Id + "')",  
            data: JSON.stringify(userPatch)
        };
    }

    insert(dm, value, notknown, tableName) {

        var userPost = {};
        userPost.Fullname = value.Firstname + ' ' + value.Surname;
        userPost.Firstname = value.Firstname;
        userPost.Surname = value.Surname;
        userPost.AllowedInstanceJson = value.InstanceUsers;
        userPost.AllowedServiceJson = value.ServiceUsers;
        userPost.Role = value.Role;
        userPost.Email = value.Email;
        userPost.Position = value.Position;
        userPost.TutorStripeId = value.TutorStripeId;
        userPost.TutorEmail = value.TutorEmail;


        return {
            type: 'POST',
            url: "/odata/Users",  
            data: JSON.stringify(userPost)
        };
    } 

    remove(dm, value, keyField, key) {

        //NOTE: This is not currently used as we ovveride the delete prompt to give more information
        // Kept in project as an example of overrding the data adpator
        return {
            type: 'DELETE',
            url: "/odata/users('" + value.Id + "')",
        };
    }

    processResponse() {

        var original = super.processResponse.apply(this, arguments);

        if (arguments[4].type === "PATCH" || arguments[4].type === "POST" || arguments[4].type === "DELETE") {
            setTimeout(RefreshGrid, 0);
        }

        return original;
    }


}

//HACK: this method is need because the custom adapter doesnt call the actionComplete on Grid when done
function RefreshGrid() {
    userGrid.actionComplete({ requestType: 'save' })
}


function InituserGrid() {

    var searchSettings = null;

    userGrid = new ej.grids.Grid({
        dataSource: userDataManager,
        query: new ej.data.Query().expand("TutorialSubjectTutorUsers($expand=TutorialSubject)"),
        searchSettings: searchSettings,
        editSettings: { showDeleteConfirmDialog: true, allowEditing: true, allowAdding: true, allowDeleting: userRole === 'Admin', mode: 'Dialog', newRowPosition: 'Top' },
        allowPaging: true,
        allowFiltering: true,
        allowSorting: true,
        allowResizing: true,
        allowExcelExport: true,
        allowPdfExport: true,
        pageSettings: { pageCount: 4, pageSize: 20 },
        searchSettings: { fields: ['Fullname', 'Email', 'Position'] },
        allowGrouping: false,
        groupSettings: { disablePageWiseAggregates: true },
        sortSettings: { columns: [{ field: 'Fullname', direction: 'Ascending' }] },
        filterSettings: {
            type: 'Excel'
        },
        toolbar: [
            'Add', 'Edit', 'Update', 'Cancel',
            { text: 'Delete', tooltipText: 'Delete', prefixIcon: 'e-icons e-delete', id: 'DeleteRecord' },
            'ExcelExport',
            'PdfExport',
            'CsvExport',
            'Search'
        ],
        width: 'auto',
        dataBound: function () {
            userGrid.autoFitColumns();
        },
        actionBegin: function (args) {

            if (args.requestType == "beginEdit" || args.requestType == 'add') {
             

           
            } else if (args.requestType == "save" || args.requestType == "cancel") {
               

        
            }
        },
        actionComplete: function (args) {

            if ((args.requestType === 'beginEdit' || args.requestType === 'add')) {

                args.dialog.width = "600px";
                args.dialog.header = args.requestType === 'beginEdit' ? 'Edit ' + args.rowData['Fullname'] : 'New Customer';

                if (ej.base.Browser.isDevice) {
                    args.dialog.height = window.innerHeight - 90 + 'px';
                    args.dialog.dataBind();
                }
            }

            else if (args.requestType === 'save' || args.requestType === 'cancel' || args.requestType === 'delete') {
            
                userGrid.refresh();

            }
        },

        columns: [


            {
                field: 'Id',
                isPrimaryKey: true,
                isIdentity: true,
                visible: false,
                allowEditing: false,

            },

            {
                field: 'Firstname',
                headerText: 'First Name',
            },

            {
                field: 'Surname',
                headerText: 'Surname',

            },

            {
                field: 'Email',
                headerText: 'Email',
            },

            {
                field: 'Position',
                headerText: 'Position',
            },

            {
                field: 'Role',
                headerText: 'User Role',
                edit: {
                    create: function ()
                    {
                        roleDropDownElement = document.createElement('input');
                        return roleDropDownElement;
                    },
                    read: function ()
                    {
                        return roleDropDown.value;
                    },
                    destroy: function ()
                    {
                        roleDropDown.destroy();
                    },
                    write: function (args)
                    {

                        roleDropDown = new ej.dropdowns.DropDownList({
                            value: args.rowData.Role,
                            popupHeight: '300px',
                            floatLabelType: 'Always',
                            dataSource: rolesDataManager,
                            fields: { text: 'Name', value: 'Name' },
                            placeholder: 'Role',

                        });

                        roleDropDown.appendTo(roleDropDownElement);
                    }
                }
            },



            {

                field: 'TutorialSubjectTutorUsers',
                headerText: 'Tutorial Subjects',
                disableHtmlEncode: false,
                valueAccessor: function (field, data, column) {
                    
                    var tutorialSubjectTutorUsers = data.TutorialSubjectTutorUsers;
                    if (!tutorialSubjectTutorUsers) tutorialSubjectTutorUsers = [];
                    var html = '';

                    for (var i = 0; i < tutorialSubjectTutorUsers.length; i++) {
                        html += tutorialSubjectTutorUsers[i].TutorialSubject.Name;
                        if (i + 1 < tutorialSubjectTutorUsers.length) html += '<br />';
                    }

                    return html;

                },
                edit: {
                    create: function () {
                        tutorialSubjectTutorUserElement = document.createElement('input');
                        return tutorialSubjectTutorUserElement;
                    },
                    read: function () {

                        var data = JSON.stringify(tutorialSubjectTutorUserList.value);
                        console.log("Instacnes: " + data);
                        return data;

                    },
                    destroy: function () {
                        tutorialSubjectTutorUserList.destroy();
                    },
                    write: function (args) {

                        var tutorialSubjectTutorUsers = args.rowData.TutorialSubjectTutorUsers;
                        if (!tutorialSubjectTutorUsers) tutorialSubjectTutorUsers = [];
                        var json = [];

                        for (var i = 0; i < tutorialSubjectTutorUsers.length; i++)
                        {
                            json.push(tutorialSubjectTutorUsers[i].TutorialSubject.Id)
                        }


                        tutorialSubjectTutorUserList = new ej.dropdowns.MultiSelect({
                            value: json,
                            dataSource: tutorialSubjectDataManager,
                            query: new ej.data.Query(),
                            fields: { text: 'Name', value: 'Id' },
                            placeholder: 'Select Tutorial Subject',
                            allowCustomValue: true,
                            mode: 'Box'
                        });

                        tutorialSubjectTutorUserList.appendTo(tutorialSubjectTutorUserElement);

                    }
                }

            },


            {
                field: 'TutorStripeId',
                headerText: 'Stripe Account Id',

            },


            {
                field: 'TutorEmail',
                headerText: 'Tutor Email',

            },
            


        ],

        toolbarClick: async function (args) {


            if (args.item.id === 'DeleteRecord') {
                ConfirmDeleteRecord();
            }

            if (args.item.id === 'Grid_pdfexport') {
                var exportProperties = {
                    pageOrientation: 'Landscape',
                    pageSize: 'A3',
                    isAutoFit: true
                };
                userGrid.pdfExport(exportProperties);
            }

            if (args.item.id === 'Grid_excelexport') {
                userGrid.excelExport();
            }

            if (args.item.id === 'Grid_csvexport') {
                userGrid.csvExport();
            }

        },


    });
    userGrid.appendTo('#Grid');





}

function InitPageControls() {



}


function ConfirmDeleteRecord() {

    var selectedRows = userGrid.getSelectedRecords();

    if (selectedRows.length === 1) {

        var selectedRow = selectedRows[0];

        ShowDialog(
            "Delete Record",
            "Are you sure you wish to delete record: " + selectedRow.Fullname + " ?",
            [
                {
                    'click': () => {
                        DeleteRecord(selectedRow.Id);
                        CloseDialog();
                    },
                    buttonModel: {
                        isPrimary: true,
                        content: 'Yes'
                    }
                },
                {
                    'click': () => {
                        CloseDialog()
                    },
                    buttonModel: {
                        content: 'No'
                    }
                }
            ]

        );

    } else {

        ShowDialog(
            "Record not Selected",
            "No record has been selected.",
            [
                {
                    'click': () => {
                        activeDialog.hide();
                    },
                    buttonModel: {
                        isPrimary: true,
                        content: 'OK'
                    }
                },

            ]

        );
    }

}

function DeleteRecord(id) {


    var url = "/odata/users('" + id + "')";
    $('.ibox-content').toggleClass('sk-loading');

    $.ajax({
        url: url,
        type: "DELETE",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            userGrid.refresh();
            $('.ibox-content').toggleClass('sk-loading');

        },
        error: function (xhRequest, ErrorText, thrownError) {

            console.log(ErrorText);
            alert("Error: Could not load data. See browser log for details.");
            $('.ibox-content').toggleClass('sk-loading');

        }
    });


}
