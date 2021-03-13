 //For jquery.flowchart reference see: https://github.com/sdrdis/jquery.flowchart

var selectedTask;
var selectedTaskOutput;
var selectedTaskReference;

$(document).ready(function () {


    var $flowchart = $('#flowchart-taskroute');

    var $container = $flowchart.parent();

    var cx = $flowchart.width() / 2;
    var cy = $flowchart.height() / 2;


    // Panzoom initialization...
    $flowchart.panzoom();

    // Centering panzoom
    $flowchart.panzoom('pan', -cx + $container.width() / 2, -cy + $container.height() / 2);

    // Panzoom zoom handling...
    //var possibleZooms = [0.5, 0.75, 1, 2, 3];
    //var currentZoom = 2;
    //$container.on('mousewheel.focal', function (e) {
    //    e.preventDefault();
    //    var delta = (e.delta || e.originalEvent.wheelDelta) || e.originalEvent.detail;
    //    var zoomOut = delta ? delta < 0 : e.originalEvent.deltaY > 0;
    //    currentZoom = Math.max(0, Math.min(possibleZooms.length - 1, (currentZoom + (zoomOut * 2 - 1))));
    //    $flowchart.flowchart('setPositionRatio', possibleZooms[currentZoom]);
    //    console.log('Panning to zoom level ' + currentZoom);
    //    $flowchart.panzoom('zoom', possibleZooms[currentZoom], {
    //        animate: false,
    //        focal: e
    //    });
    //});

    //var data = {
    //    operators: {
    //        operator1: {
    //            top: cy - 100,
    //            left: cx - 200,
    //            properties: {
    //                title: 'Operator 1',
    //                inputs: {},
    //                outputs: {
    //                    output_1: {
    //                        label: 'Output 1',
    //                    }
    //                }
    //            }
    //        },
    //        operator2: {
    //            top: cy,
    //            left: cx + 140,
    //            properties: {
    //                title: 'Operator 2',
    //                inputs: {
    //                    input_1: {
    //                        label: 'Input 1',
    //                    },
    //                    input_2: {
    //                        label: 'Input 2',
    //                    },
    //                },
    //                outputs: {}
    //            }
    //        },
    //    },
    //    links: {
    //        link_1: {
    //            fromOperator: 'operator1',
    //            fromConnector: 'output_1',
    //            toOperator: 'operator2',
    //            toConnector: 'input_2',
    //        },
    //    }
    //};

    $flowchart.flowchart({

        defaultLinkColor: '#1ab394',
        onOperatorDelete: function (operatorId) {

            var operatorData = $flowchart.flowchart('getOperatorData', operatorId);

            if (operatorData.properties.taskId > 0) {

                DeleteTask(operatorData.properties.taskId);

            }

            return true;
        },
        onLinkCreate: function (linkId, linkData) {

            var fromOperator = $flowchart.flowchart('getOperatorData', linkData.fromOperator);
            var toOperator = $flowchart.flowchart('getOperatorData', linkData.toOperator);

            if (!isLoading) {
                CreateLink(fromOperator.properties.taskId, toOperator.properties.taskId);
            }

            return true;
        },
        onLinkDelete: function (linkId, forced) {

            var linkData = $flowchart.flowchart('getLinkData', linkId);

            var fromOperator = $flowchart.flowchart('getOperatorData', linkData.fromOperator);
            var toOperator = $flowchart.flowchart('getOperatorData', linkData.toOperator);
            DeleteLink(fromOperator.properties.taskId, toOperator.properties.taskId);

            return true;
        },
        onOperatorMoved: function (operator, position) {

            SaveRouteData();

        },
        onOperatorSelect: function (operatorId) {

            var operatorData = $flowchart.flowchart('getOperatorData', operatorId);
            DisplayTask(operatorData.properties.taskId);
            return true;

        }
    });

    
    function showEvent(message) {

        alert(message);

    }


    function SetData(json) {

        var data = JSON.parse(json);
        $flowchart.flowchart('setData', data);
    }

    function GetData() {

        var $flowchart = $('#flowchart-taskroute');

        var data = $flowchart.flowchart('getData');
        return JSON.stringify(data, null, 2);

    }


    function uuidv4() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    var $draggableOperators = $('.draggable_operator');

    function getOperatorData($element) {

        var operatorData = {
            properties: {
                title: $element.text(),
                inputs: {
                    input_1: {
                        label: 'Input',
                        multiple: true
                    }
                },
                outputs: {
                    output_1: {
                        label: 'Output',
                        multiple: true
                    }
                },
                processId: parseInt($element[0].id.replace('process_', ''))
            }
        };

        return operatorData;
    }

    var operatorId = 0;

    $draggableOperators.draggable({
        cursor: "move",
        opacity: 0.7,

        helper: 'clone',
        appendTo: 'body',
        zIndex: 1000,

        helper: function (e) {
            var $this = $(this);
            var data = getOperatorData($this);
            return $flowchart.flowchart('getOperatorElement', data);
        },
        stop: function (e, ui) {
            var $this = $(this);
            var elOffset = ui.offset;
            var containerOffset = $container.offset();
            if (elOffset.left > containerOffset.left &&

                elOffset.top > containerOffset.top &&
                elOffset.left < containerOffset.left + $container.width() &&
                elOffset.top < containerOffset.top + $container.height()) {

                var flowchartOffset = $flowchart.offset();

                var relativeLeft = elOffset.left - flowchartOffset.left;
                var relativeTop = elOffset.top - flowchartOffset.top;

                var positionRatio = $flowchart.flowchart('getPositionRatio');
                relativeLeft /= positionRatio;
                relativeTop /= positionRatio;

                var data = getOperatorData($this);
                data.left = relativeLeft;
                data.top = relativeTop;

                var operatorId = uuidv4();

                data.properties.operatorGuid = operatorId;
                data.properties.taskId = -1;
                data.properties.taskRouteId = taskRouteId;

                $flowchart.flowchart('createOperator', operatorId, data);

                CreateTask(operatorId, data.properties.processId, taskRouteId, data.properties.title);


            }
        }
    });

 
});

function CreateLink(fromTaskId, toTaskId) {


    var url = "/api/tasklink/create";
    var model = {
        FromTaskId: fromTaskId,
        ToTaskId: toTaskId
    };

    $.ajax({
        url: url,
        data: model,
        type: "POST",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            SaveRouteData();
        },
        error: function (xhRequest, ErrorText, thrownError) {

            console.log(ErrorText);
            //alert(ErrorText);

        }
    });


}

function DeleteLink(fromTaskId, toTaskId) {


    var url = "/api/tasklink/delete";
    var model = {
        FromTaskId: fromTaskId,
        ToTaskId: toTaskId
    };

    $.ajax({
        url: url,
        type: "POST",
        data: model,
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            SaveRouteData();
        },
        error: function (xhRequest, ErrorText, thrownError) {

            console.log(ErrorText);
            alert(ErrorText);

        }
    });



}


function CreateTask(operatorId, processId, taskRouteId, title) {


    var url = "/api/task/savetask";
    var model = {
        Id: -1,
        ProcessId: processId,
        TaskRouteId: taskRouteId,
        Name: title,
        OperatorGuid: operatorId,
        IsRequired: true
    };

    $.ajax({
        url: url,
        data: model,
        type: "POST",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            var $flowchart = $('#flowchart-taskroute');
            var operatordata = $flowchart.flowchart('getOperatorData', data.OperatorGuid);
            operatordata.properties.taskId = data.Id;
            $flowchart.flowchart('setOperatorData', data.OperatorGuid, operatordata);

            var titleHtml = data.Name + " &nbsp;<span class='badge badge-warning'>Required</span>";
            $flowchart.flowchart('setOperatorTitle', data.OperatorGuid, titleHtml);

            SaveRouteData();
        },
        error: function (xhRequest, ErrorText, thrownError) {

            console.log(ErrorText);
            alert(ErrorText);

        }
    });

} 

function DeleteTask(taskId) {


    var url = "/api/task?id=" + taskId;

    $.ajax({
        url: url,
        type: "DELETE",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            SaveRouteData();
        },
        error: function (xhRequest, ErrorText, thrownError) {

            console.log(ErrorText);
            alert(ErrorText);

        }
    });


}

function SaveRouteData() {

    UpdateFlow();

    var url = "/api/taskroute/saveroutedata";
    var model = {
        Id: taskRouteId,
        FlowJson: $('#FlowJson').val()
    };

    $.ajax({
        url: url,
        data: model,
        type: "POST",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

        },
        error: function (xhRequest, ErrorText, thrownError) {

            console.log(ErrorText);
            alert(ErrorText);

        }
    });

 
}   

function UpdateFlow() {

    var $flowchart = $('#flowchart-taskroute');

    var data = $flowchart.flowchart('getData');
    var json = JSON.stringify(data, null, 2);

    $('#FlowJson').val(json);

}

function SaveFlow() {

    UpdateFlow();
    $('#FlowForm').submit();

}



function DeleteSelected() {

    var $flowchart = $('#flowchart-taskroute');
    $flowchart.flowchart('deleteSelected');
    $('#selectedTask').hide(true);

    var button = $('#processes-collapse');
    if (button.children().first().hasClass("fa-chevron-down")) {
        button.click();
    }
}


function DisplayTask(taskId) {

    $('#task-saved').hide(false);

    var url = "/api/task/gettask?taskId=" + taskId;
   

    $.ajax({
        url: url,
        type: "GET",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            selectedTask = data;

            RenderTask(data);

            

        },
        error: function (xhRequest, ErrorText, thrownError) {

            console.log(ErrorText);
            alert(ErrorText);

        }
    });
}


function RenderTask(data) {


    $('#task-id').text(data.Id);
    $('#task-name').val(data.Name);
    $('.task-name').text(data.Name);

    $('#task-required').iCheck(data.IsRequired ? 'check' : 'uncheck');
    $('#task-alert').iCheck(data.IsAlert ? 'check' : 'uncheck');

    $('#task-parallel').iCheck(data.IsParallel ? 'check' : 'uncheck');
    $('#task-floating').iCheck(data.IsFloating ? 'check' : 'uncheck');
    $('#task-ignoreifsecondary').iCheck(data.IgnoreIfSecondary ? 'check' : 'uncheck');
 
    $('#task-routetstart').iCheck(data.IsTaskRouteStart ? 'check' : 'uncheck');
    $('#task-routetend').iCheck(data.IsTaskRouteEnd ? 'check' : 'uncheck');
    $('#selectedTask').show(true);

    var button = $('#processes-collapse');
    if (button.children().first().hasClass("fa-chevron-up")) {
        button.click();
    }

    ////Render Task Outputs
    $('#taskoutputlist').empty();
    var templateHtml = '';

    if (data.TaskOutputs !== null) {

        for (var i = 0; i < data.TaskOutputs.length; i++) {

            var outputTemplate = $('#taskoutputitem').html();
            outputTemplate = outputTemplate.replace(/{{PUBLIC}}/g, data.TaskOutputs[i].IsCustomerOutput ? "primary" : "warning");
            outputTemplate = outputTemplate.replace(/{{PUBLICLABEL}}/g, data.TaskOutputs[i].IsCustomerOutput ? "Public" : "Internal");
            outputTemplate = outputTemplate.replace(/{{NAME}}/g, data.TaskOutputs[i].Name);
            outputTemplate = outputTemplate.replace(/{{OUTPUT-ID}}/g, data.TaskOutputs[i].Id);
 
            templateHtml += outputTemplate;
        }

    }

    $('#taskoutputlist').html(templateHtml);


    $('#taskreferencelist').empty();
    var templateReferenceHtml = '';

    if (data.TaskReferences !== null) {

        for (var r = 0; r < data.TaskReferences.length; r++) {

            var referenceTemplate = $('#taskreferenceitem').html();
            referenceTemplate = referenceTemplate.replace(/{{NAME}}/g, data.TaskReferences[r].Name);
            referenceTemplate = referenceTemplate.replace(/{{OUTPUT-ID}}/g, data.TaskReferences[r].Id);
            referenceTemplate = referenceTemplate.replace(/{{URL}}/g, data.TaskReferences[r].Url === null || data.TaskReferences[r].Url.length === 0 ? "Text" : "Link");

            templateReferenceHtml += referenceTemplate;
        }

    }


    $('#taskreferencelist').html(templateReferenceHtml);

    


}


function SaveTask() {

    var url = "/api/task/savetask";

    selectedTask.Name = $('#task-name').val();
    selectedTask.IsAlert = $('#task-alert').iCheck('update')[0].checked;

    selectedTask.IsRequired = $('#task-required').iCheck('update')[0].checked;
    selectedTask.IsSkipped = !selectedTask.IsRequired;
   
    selectedTask.IsTaskRouteStart = $('#task-routetstart').iCheck('update')[0].checked;
    selectedTask.IsTaskRouteEnd = $('#task-routetend').iCheck('update')[0].checked;

    selectedTask.IsParallel = $('#task-parallel').iCheck('update')[0].checked;
    selectedTask.IsFloating = $('#task-floating').iCheck('update')[0].checked;
    selectedTask.IgnoreIfSecondary = $('#task-ignoreifsecondary').iCheck('update')[0].checked;


    $.ajax({
        url: url,
        data: selectedTask,
        type: "POST",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            var $flowchart = $('#flowchart-taskroute');

            var titleHtml = data.Name;

            if (data.IsTaskRouteStart) {

                titleHtml += " &nbsp;<span class='badge badge-primary'>SRT</span>";
            }

            if (data.IsTaskRouteEnd) {

                titleHtml += " &nbsp;<span class='badge badge-primary'>END</span>";
            }

            if (data.IsFloating) {

                titleHtml += " &nbsp;<span class='badge badge-success'>FLT</span>";
            }

            if (data.IsParallel) {

                titleHtml += " &nbsp;<span class='badge badge-success'>PAR</span>";
            }

            if (!data.IsRequired) {

                titleHtml += " &nbsp;<span class='badge badge-success'>OPT</span>";
            } else {
                titleHtml += " &nbsp;<span class='badge badge-warning'>REQ</span>";

            }

            if (data.IsAlert) {

                titleHtml += " &nbsp;<span class='badge badge-danger'>ALT</span>";
            }

            var operatordata = $flowchart.flowchart('setOperatorTitle', data.OperatorGuid, titleHtml);

            $('#task-saved').show(true);

            SaveRouteData();

        },
        error: function (xhRequest, ErrorText, thrownError) {

            console.log(ErrorText);
            alert(ErrorText);

        }
    });
}



function ToogleCheckList(checkListEnabled) {

    if (checkListEnabled) {
        $("#select-checklist-div").show(true);
    } else {
        $("#select-checklist-div").hide(true);
    }

}


function SaveTaskOutput() {

    var url = "/api/taskoutput/save";

    var selectedTaskOutputId = selectedTaskOutput === null ? -1 : selectedTaskOutput.Id;

    var data = {

        Id: selectedTaskOutputId,
        Name: $('#outputdocument-name').val(),
        TaskId: parseInt($('#task-id').text()),
        IsCustomerOutput: $('#outputdocument-iscustomeroutput').iCheck('update')[0].checked,
        IsOptional: $('#outputdocument-isoptional').iCheck('update')[0].checked,
        TaskOutputType: parseInt($('#select-output-type option:selected').val()),
        DocumentOutputTemplateId: parseInt($('#select-output-template option:selected').val()),
        ConsumableGroupId: parseInt($('#select-consumablegroup option:selected').val())
        
    };


    $.ajax({
        url: url,
        data: data,
        type: "POST",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            $('#modalDocumentOutput').modal('hide');
            DisplayTask(data.TaskId);
        },
        error: function (xhRequest, ErrorText, thrownError) {

            console.log(ErrorText);
            alert(ErrorText);
            $('#modalDocumentOutput').modal('hide');
        }
    });

}


function AddTaskOutput() {

    selectedTaskOutput = null;

    $("#modalDocumentOutputTitle").text('Add Task Output');

    $('#modalDocumentOutput').modal('show');

    $("#outputdocument-name").val('');


    $("#select-outputdocumenttype").prop('selectedIndex', 0);
    $("#select-checklist").prop('selectedIndex', 0);
    $("#select-checklist-div").hide();

 

}





function EditTaskOutput(taskOutputId) {

    $("#modalDocumentOutputTitle").text('Edit Task Output');


    for (var i = 0; i < selectedTask.TaskOutputs.length; i++) {

        if (selectedTask.TaskOutputs[i].Id === taskOutputId) {
            selectedTaskOutput = selectedTask.TaskOutputs[i];
            break;
        }

    }

    $("#outputdocument-name").val(selectedTaskOutput.Name);

    $('#select-output-type').val(selectedTaskOutput.TaskOutputType);
    $('#select-output-template').val(selectedTaskOutput.DocumentOutputTemplateId);

    $('#select-consumablegroup').val(selectedTaskOutput.ConsumableGroupId);

    selectedTaskOutput.IsCustomerOutput ? $('#outputdocument-iscustomeroutput').iCheck('check') : $('#outputdocument-iscustomeroutput').iCheck('uncheck');
    selectedTaskOutput.IsOptional ? $('#outputdocument-isoptional').iCheck('check') : $('#outputdocument-isoptional').iCheck('uncheck');

    $("#select-checklist").prop('selectedIndex', 0);
    $("#select-checklist-div").hide();

    DisplayOputputOptions();

    $('#modalDocumentOutput').modal('show');

}


function DisplayOputputOptions() {

    var outputSelectedValue = parseInt($("#select-output-type").val());

    if (outputSelectedValue === 101) {

        $("#DivConsumableGroup").show();
        $("#DivOutputTemplate").hide();

    } else if (outputSelectedValue === 102) {

        $("#DivConsumableGroup").hide();
        $("#DivOutputTemplate").show();

    } else {

        $("#DivConsumableGroup").hide();
        $("#DivOutputTemplate").hide();

    }

}


function DeleteTaskOutput(taskOutputId) {


    var url = "/api/taskoutput/delete";

    var selectedTaskOutputId = selectedTaskOutput == null ? -1 : selectedTaskOutput.Id;

    var data = {

        Id: taskOutputId

    };

    $.ajax({
        url: url,
        data: data,
        type: "POST",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            $('#modalDocumentOutput').modal('hide');
            DisplayTask(selectedTask.Id);
        },
        error: function (xhRequest, ErrorText, thrownError) {

            console.log(ErrorText);
            alert(ErrorText);
            $('#modalDocumentOutput').modal('hide');
        }
    });

}



function AddTaskReference() {

    selectedTaskReference = null;

    $("#modalDocumentReferenceTitle").text('Add Task Reference');

    $('#modalDocumentReference').modal('show');

    $("#referencedocument-name").val('');
    $("#referencedocument-notes").val('');
    $("#referencedocument-url").val('');

}





function EditTaskReference(taskReferenceId) {

    $("#modalDocumentReferenceTitle").text('Edit Task Reference');


    for (var i = 0; i < selectedTask.TaskReferences.length; i++) {

        if (selectedTask.TaskReferences[i].Id === taskReferenceId) {
            selectedTaskReference = selectedTask.TaskReferences[i];
            break;
        }

    }

    $("#referencedocument-name").val(selectedTaskReference.Name);
    $("#referencedocument-notes").val(selectedTaskReference.Notes);
    $("#referencedocument-url").val(selectedTaskReference.Url);

    $('#modalDocumentReference').modal('show');




}


function SaveTaskReference() {

    var url = "/api/taskreference/save";

    var selectedTaskReferenceId = selectedTaskReference == null ? -1 : selectedTaskReference.Id;

    var data = {

        Id: selectedTaskReferenceId,
        Name: $('#referencedocument-name').val(),
        Notes: $("#referencedocument-notes").val(),
        Url: $("#referencedocument-url").val(),
        TaskId: parseInt($('#task-id').text())

    };

    $.ajax({
        url: url,
        data: data,
        type: "POST",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            $('#modalDocumentReference').modal('hide');
            DisplayTask(data.TaskId);
        },
        error: function (xhRequest, ErrorText, thrownError) {

            console.log(ErrorText);
            alert(ErrorText);
            $('#modalDocumentReference').modal('hide');
        }
    });

}


function DeleteTaskReference(taskReferenceId) {


    var url = "/api/taskreference/delete";

    var selectedTaskReferenceId = selectedTaskReference == null ? -1 : selectedTaskReference.Id;

    var data = {

        Id: taskReferenceId

    };

    $.ajax({
        url: url,
        data: data,
        type: "POST",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            $('#modalDocumentReference').modal('hide');
            DisplayTask(selectedTask.Id);
        },
        error: function (xhRequest, ErrorText, thrownError) {

            console.log(ErrorText);
            alert(ErrorText);
            $('#modalDocumentReference').modal('hide');
        }
    });

}
