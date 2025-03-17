var groupTutorialsDataManager;
var groupTutorialsGrid;

var groupTutorialDescriptionElement;
var groupTutorialDescription;

$(document).ready(function ()
{
    InitDataManagers();
    InitGrid();

});

function InitDataManagers()
{

    groupTutorialsDataManager = new ej.data.DataManager({
        url: '/odata/grouptutorials',
        adaptor: new ej.data.ODataV4Adaptor(),
        crossDomain: true
    });


    tutorUserDataManager = new ej.data.DataManager({
        url: '/odata/users',
        adaptor: new UserDataManagerAdaptor(),
        crossDomain: true
    });

}

function InitGrid()
{

    groupTutorialsGrid = new ej.grids.Grid({
        dataSource: groupTutorialsDataManager,
        query: new ej.data.Query(),
        editSettings: { showDeleteConfirmDialog: true, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Dialog', newRowPosition: 'Top' },
        allowPaging: true,
        allowSorting: true,
        sortSettings: { columns: [{ field: 'DateTimeStart', direction: 'Descending' }] },
        allowResizing: true,
        allowExcelExport: true,
        allowPdfExport: true,
        allowFiltering: true,
        filterSettings: {
            type: 'Excel',
        },
        pageSettings: { pageCount: 4, pageSize: 50 },
        toolbar: ['Add', 'Edit', 'Delete', 'Update', 'Cancel'],
        width: 'auto',
        actionBegin: function (args)
        {


            if (args.requestType === "beginEdit" || args.requestType === 'add')
            {
                this.columns[1].visible = false;
                this.columns[8].visible = false;
                this.columns[9].visible = false;


            } 
            else if (args.requestType === "save" || args.requestType === "cancel")
            {
                this.columns[1].visible = true;
                this.columns[8].visible = true;
                this.columns[9].visible = true;

            }
        },

        columns: [


            {
                field: 'AppId',
                allowEditing: false,
                defaultValue: 1,
                visible: false

            },


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
                width: 300,
                allowSorting: false
            },


            {
                field: 'Description',
                headerText: 'Description',
                clipMode: 'EllipsisWithTooltip',
                validationRules: { required: true },
                width: 300,
                valueAccessor: function (field, data, column)
                {

                    return data.Description;

                },
                edit: {
                    create: function ()
                    {
                        groupTutorialDescriptionElement = document.createElement('textarea');
                        return groupTutorialDescriptionElement;
                    },
                    read: function ()
                    {

                        var data = groupTutorialDescription.value;
                        console.log("Description: " + data);
                        return data;
                    },
                    destroy: function ()
                    {
                        groupTutorialDescription.destroy();
                    },
                    write: function (args)
                    {


                        groupTutorialDescription = new ej.inputs.TextBox({

                            value: args.rowData.Description,
                            floatLabelType: 'Auto',
                            placeholder: 'Description',

                        });
                        groupTutorialDescription.appendTo(groupTutorialDescriptionElement);


                    }
                }
            },

            {
                field: 'TutorUserId',
                foreignKeyField: 'Id',
                foreignKeyValue: 'Fullname',
                dataSource: tutorUserDataManager,
                query: new ej.data.Query().where('Deleted', 'equal', false).where('IsTutor', 'equal', true),
                headerText: 'Tutor',
                allowSorting: false

            },

            {

                field: 'TutorialCostPerPerson',
                headerText: 'Price Per Person',
                minWidth: 125,
                format: "n2",
                defaultValue: 0,
                validationRules: { required: true },
                editType: 'numericedit'
            },

            {
                field: 'DateTimeStart',
                headerText: 'Date / Time Start',
                validationRules: { required: true },
                format: { type: 'dateTime', format: 'dd/MM/yyyy HH:mm' },
                editType: 'datetimepickeredit',
                width: 200

            },


            {
                field: 'DateTimeEnd',
                headerText: 'Date / Time End',
                validationRules: { required: true },
                format: { type: 'dateTime', format: 'dd/MM/yyyy HH:mm' },
                editType: 'datetimepickeredit',

                width: 200

            },


             {
                 field: 'TeamsId',
                 headerText: 'Teams Id',
                 allowEditing: false,
                width: 150,
                allowSorting: false
            },

            {
                field: 'TeamsLink',
                headerText: 'Teams Link',
                allowEditing: false,
                
            },

        ],

        actionComplete: function (args)
        {

            if (args.requestType === 'delete' || args.requestType === 'save')
            {
                groupTutorialsGrid.refresh();
                return;
            }

        }
    });

    groupTutorialsGrid.appendTo('#Grid');

}


function actionBegin(args, target)
{
    if (args.requestType === 'save')
    {
        if (target.pageSettings.currentPage !== 1 && target.editSettings.newRowPosition === 'Top')
        {
            args.index = (target.pageSettings.currentPage * target.pageSettings.pageSize) - target.pageSettings.pageSize;
        } else if (target.editSettings.newRowPosition === 'Bottom')
        {
            args.index = (target.pageSettings.currentPage * target.pageSettings.pageSize) - 1;
        }
    }
}

class UserDataManagerAdaptor extends ej.data.ODataV4Adaptor
{

    processQuery() {

        var query = super.processQuery.apply(this, arguments);
        var url = query.url.replaceAll(" eq ", " eq '").replaceAll(")", "')")

        return {
            type: "GET",
            url: url,
            ejPvtData: this.pvt
        };

    }


}