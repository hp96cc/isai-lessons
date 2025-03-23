var tutorialsDataManager;
var tutorialsGrid;

var tutorialDescriptionElement;
var tutorialDescription;

$(document).ready(function ()
{
    InitDataManagers();
    InitGrid();

});

function InitDataManagers()
{

    tutorialsDataManager = new ej.data.DataManager({
        url: '/odata/tutorials',
        adaptor: new ej.data.ODataV4Adaptor(),
        crossDomain: true
    });


}

function InitGrid()
{

    tutorialsGrid = new ej.grids.Grid({
        dataSource: tutorialsDataManager,
        query: new ej.data.Query().expand("Customer,TutorUser,GroupTutorial,Lesson"),
        editSettings: { showDeleteConfirmDialog: true, allowEditing: false, allowAdding: false, allowDeleting: true, mode: 'Dialog', newRowPosition: 'Top' },
        allowPaging: true,
        allowGrouping: true,
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
        toolbar: [ 'Delete',  'ExcelExport', 'PdfExport', 'CsvExport'],
        width: 'auto',
        dataBound: function ()
        {
            tutorialsGrid.autoFitColumns();
        },
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
                field: 'Id',
                isPrimaryKey: true,
                isIdentity: true,
                headerText: 'Id',
                allowEditing: false,
                defaultValue: 0,
                width: 70,
            },


            {
                field: 'CustomerId',
                headerText: 'Customer',
                validationRules: { required: true },
                width: 200,
                allowSorting: false, valueAccessor: function (field, data, column)
                {

                    return data.Customer.FirstName + ' ' + data.Customer.LastName;

                },
            },


            {
                field: 'Customer.Email',
                headerText: 'Customer Email',
                validationRules: { required: true },
                width: 200,
                allowSorting: false, valueAccessor: function (field, data, column)
                {

                    return data.Customer.Email;

                },
            },


            {
                field: 'TutorUser.Fullname',
                headerText: 'Tutor',
                validationRules: { required: true },
                width: 200,
                allowSorting: false, valueAccessor: function (field, data, column)
                {

                    return data.TutorUser.Fullname;

                },
            },


            {
                field: 'GroupTutorial.Name',
                headerText: 'Group Tutorial',
                validationRules: { required: true },
                width: 200,
                allowSorting: false, valueAccessor: function (field, data, column)
                {
                    if (data.GroupTutorialId)
                    {
                        return data.GroupTutorial.Name;
                    } else
                    {
                        return '';
                    }

                },
            },

            {
                field: 'LessonId',
                headerText: 'Lesson',
                validationRules: { required: true },
                width: 200,
                allowSorting: false, valueAccessor: function (field, data, column)
                {
                    if (data.LessonId)
                    {
                        return data.Lesson.Name;
                    } else
                    {
                        return '';
                    }

                },
            },

            {
                field: 'Name',
                headerText: 'Name',
                validationRules: { required: true },
                width: 350,
                allowSorting: false
            },

            {
                field: 'StripePaymentId',
                headerText: 'Stripe Payment Id',
                validationRules: { required: true },
                width: 200,
                allowSorting: false
            },




            {

                field: 'TutorialCost',
                headerText: 'Cost',
                minWidth: 125,
                format: "n2",
                defaultValue: 0,
                validationRules: { required: true },
                editType: 'numericedit'
            },

            {

                field: 'DurationInMinutes',
                headerText: 'Duration',
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

        toolbarClick: async function (args)
        {


            if (args.item.id === 'Grid_pdfexport')
            {

                tutorialsGrid.pdfExport();
            }

            if (args.item.id === 'Grid_excelexport')
            {
                tutorialsGrid.excelExport();
            }

            if (args.item.id === 'Grid_csvexport')
            {
                tutorialsGrid.csvExport();
            }

        },

        actionComplete: function (args)
        {

            if (args.requestType === 'delete' || args.requestType === 'save')
            {
                tutorialsGrid.refresh();
                return;
            }

        }
    });

    tutorialsGrid.appendTo('#Grid');

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