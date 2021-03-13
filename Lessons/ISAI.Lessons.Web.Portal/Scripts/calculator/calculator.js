var calculator;
var userCalculator;
var numberOfCopiesId;
var pagesInFileId;

var noOfSidesOption;

var copyPricedElements;
var oneOffPricedElements;
var allElements;

var templateCalculatorTotalHTML;
var templateCalculatorGroupHeaderHTML;
var templateCalculatorGroupOptionHeaderHTML;
var templateCalculatorGroupOptionTextElementHTML;
var templateCalculatorGroupOptionTextDropDownElementHTML;
var templateCalculatorGroupOptionTextDropDownElementImageHTML;
var templateCalculatorGroupOptionDivStart = '<div id="divGroupOption-{{id}}" class="form-group row m-0" {{hidden}}>';
var templateCalculatorGroupOptionDivEnd = '</div>';
var templateCalculatorFooter;

var numberOfSides;
var numberOfCopies;
var pagesInFile;
var totalCopiesCount;
var totalPagesCount;

var subTotalPrice = 0;
var setupPrice = 0;
var totalPrice = 0;
var totalWeight;

$(document).ready(function () {

    $.fn.selectpicker.Constructor.BootstrapVersion = '4';
   
    templateCalculatorTotal = $('#template-calculatortotal').html();
    templateCalculatorGroupHeaderHTML = $('#template-calculatorgroupheader').html();
    templateCalculatorGroupOptionHeaderHTML = $('#template-calculatorgroupoptionheader').html();
    templateCalculatorGroupOptionTextElementHTML = $('#template-calculatorgroupoptiontextelement').html();
    
    templateCalculatorGroupOptionTextDropDownElementHTML = $('#template-calculatorgroupoptiontextdropdownelement').html();
    templateCalculatorGroupOptionTextDropDownElementImageHTML = $('#template-calculatorgroupoptiontextdropdownelement-image').html();

    templateCalculatorFooter = $('#template-calculatorfooter').html();

    LoadCalculator();

});

async function LoadCalculator() {

    var url = baseUrl + "/api/calculator/?calculatorId=" + calculatorId;
    
    $.ajax({
        url: url,
        type: "GET",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            calculator = data;
            DisplayCalculator();
            
        },
        error: function (xhRequest, ErrorText, thrownError) {

            console.log(ErrorText);

        }
    });

}


function DisplayCalculator() {

    var calculatorHTML = '';

    numberOfCopiesId = null;
    pagesInFileId = null;
  
    allElements = [];

    calculatorHTML += Mustache.render(templateCalculatorTotal);

    if (calculator.CalculatorGroups !== null) {

        calculator.CalculatorGroups.forEach(function (calculatorGroup) {

            calculatorHTML += Mustache.render(templateCalculatorGroupHeaderHTML, { name: calculatorGroup.Name });

            calculatorGroup.CalculatorGroupOptions.forEach(function (calculatorGroupOption) {

                allElements.push(calculatorGroupOption);

                //Hidden values
                if (calculatorGroupOption.CalculatorGroupOptionTypeId === 500) {
                    calculatorHTML += RenderHidden(calculatorGroupOption);
                    return;
                }


                var divStart = templateCalculatorGroupOptionDivStart.slice().replace("{{id}}", calculatorGroupOption.Id);

                if (calculatorGroupOption.LinkedGroupOptionRule === 'type=hideonload') {
                    divStart = divStart.replace("{{hidden}}", 'style="display:none;"');
                } else {
                    divStart = divStart.replace("{{hidden}}", '');
                }

                calculatorHTML += divStart;

                calculatorHTML += Mustache.render(templateCalculatorGroupOptionHeaderHTML, { name: calculatorGroupOption.Name });

               
                switch (calculatorGroupOption.CalculatorGroupOptionTypeId) {

                    case 1:
                        //1	Textbox (Optional)
                        calculatorHTML += RenderTextBox(calculatorGroupOption, false, 'text');
                        break;

                    case 2:
                        //2	Textbox (Mandatory)
                        calculatorHTML += RenderTextBox(calculatorGroupOption, true, 'number');
                        break;

                    case 11:
                        //11	Numeric Textbox (Optional)
                        calculatorHTML += RenderTextBox(calculatorGroupOption, false, 'number');
                        break;

                    case 12:
                        //12	Numeric Textbox(Mandatory)
                        calculatorHTML += RenderTextBox(calculatorGroupOption, true, 'number');
                        break;
                    case 20:
                        //20	Pages in File
                        pagesInFileId = calculatorGroupOption.Id;

                        if (calculatorGroupOption.Increment !== 1) {

                            calculatorHTML += RenderNumberDropDown(calculatorGroupOption, calculatorGroupOption.Increment, calculatorGroupOption.InitialValue, 1000);

                        } else {

                            calculatorHTML += RenderTextBox(calculatorGroupOption, true, 'number');

                        }
                        console.log('Pages in File Id: ' + pagesInFileId);
                        break;
                    case 21:
                        //21	No of Copies
                        numberOfCopiesId = calculatorGroupOption.Id;
                       
                        if (calculatorGroupOption.Increment !== 1) {

                            calculatorHTML += RenderNumberDropDown(calculatorGroupOption, calculatorGroupOption.Increment, calculatorGroupOption.InitialValue, 1000);

                        } else {

                            calculatorHTML += RenderTextBox(calculatorGroupOption, true, 'number');

                        }


                        console.log('No of copies Id: ' + numberOfCopiesId);
                        break;
                    case 31:
                        //31	No of Copies (drop Down)
                        numberOfCopiesId = calculatorGroupOption.Id;
                        calculatorHTML += RenderDropDown(calculatorGroupOption, 'text');
                        console.log('No of copies Id: ' + numberOfCopiesId);
                        break;
                    case 22:
                        //22	No of side
                        noOfSidesOption = calculatorGroupOption;
                        calculatorHTML += RenderDropDown(calculatorGroupOption, 'text');
                        console.log('No of sides Id: ' + numberOfCopiesId);
                        break;

                    case 50:
                        //50	File Upload (Optional)
                        //51	File Upload(Mandatory)
                        calculatorHTML += RenderFileUpload(calculatorGroupOption, true);
                        break;

                    case 200:
                        //200	Text Drop Down
                        calculatorHTML += RenderDropDown(calculatorGroupOption, 'text');
                        break;

                    case 300:
                        //300	Headed Text Drop Down
                        calculatorHTML += RenderDropDown(calculatorGroupOption, 'header');
                        break;

                    case 400:
                        //400	Image Drop Down
                        calculatorHTML += RenderDropDown(calculatorGroupOption, 'image');
                        break;

                    default:
                        break;
                        

                }

                calculatorHTML += templateCalculatorGroupOptionDivEnd;


            });

        });

    }

    calculatorHTML += Mustache.render(templateCalculatorFooter);

    $('#calculator').html(calculatorHTML);
    $("#calculator-loading").hide();
    $("#calculator").show();

    $('select').selectpicker();

    CalculatePrice();


}


function RenderTextBox(calculatorGroupOption, required, type) {

    var initialValue = '';

    if (type === 'number') {

        if (calculatorGroupOption.InitialValue === undefined || calculatorGroupOption.InitialValue === null || calculatorGroupOption.InitialValue === "") {
            initialValue = '0';
        } else {
            initialValue = calculatorGroupOption.InitialValue;
        }

    } else if (type === 'number') {

        if (calculatorGroupOption.InitialValue === undefined || calculatorGroupOption.InitialValue === null || calculatorGroupOption.InitialValue === "") {
            initialValue = '';
        } else {
            initialValue = calculatorGroupOption.InitialValue;
        }
    }

    return Mustache.render(templateCalculatorGroupOptionTextElementHTML, { id: calculatorGroupOption.Id, type: type, name: calculatorGroupOption.Name, required: required ? "required" : "", initialValue: initialValue });

}


function RenderNumberDropDown(calculatorGroupOption, step, min, max) {

    var template = templateCalculatorGroupOptionTextDropDownElementHTML;

    step = parseInt(step);
    min = parseInt(min);
    max = parseInt(max);

    var groupOptionValues = [];

    for (var i = min; i < max; i += step) {

        groupOptionValues.push({

            Id: i,
            Name: i.toString()

        });
    }

    var method = "CalculatePrice();";

    return Mustache.render(template, { method: method, id: calculatorGroupOption.Id, name: calculatorGroupOption.Name, CalculatorGroupOptionValues: groupOptionValues });

}

function RenderDropDown(calculatorGroupOption, type) {

    var template;

    switch (type) {

        case 'image':
            template = templateCalculatorGroupOptionTextDropDownElementImageHTML;
            break;

        case 'header':
        default:
            template = templateCalculatorGroupOptionTextDropDownElementHTML;
            break;
    }

    var method = "CalculatePrice();";

    if (calculatorGroupOption.LinkedGroupOptionRule && calculatorGroupOption.LinkedGroupOptionRule.startsWith('type=show')) {

        var options = calculatorGroupOption.LinkedGroupOptionRule.split('&');

        var valuesToCheck = [];
        var elementsToCheckId = [];
        var elmentsToDisplayId = [];

        for (var i = 0; i < options.length; i++) {

            var optionValues = options[i].split('|');
            var valueToCheck = optionValues[1].replace('selectedId=', '');
            var elementToCheckId = "#input-" + calculatorGroupOption.Id;
            var elmentToDisplayId = "#divGroupOption-" + optionValues[2].replace('displayoption=', '');

            valuesToCheck.push(valueToCheck);
            elementsToCheckId.push(elementToCheckId);
            elmentsToDisplayId.push(elmentToDisplayId);

        }


        method = 'IfSelectedDisplayElement("' + elmentsToDisplayId.join('|') + '", "' + elementsToCheckId.join('|') + '", "' + valuesToCheck.join('|') + '")';
    }

    var html = Mustache.render(template, { method: method, id: calculatorGroupOption.Id, name: calculatorGroupOption.Name, CalculatorGroupOptionValues: calculatorGroupOption.CalculatorGroupOptionValues });

    //console.log(html);

    return html;
}

function IfSelectedDisplayElement(elementsToDisplayId, elementsToCheckId, valuesToCheck) {


    elementsToDisplay = elementsToDisplayId.split("|");
    elementsToCheck = elementsToCheckId.split("|");
    checkedValues = valuesToCheck.split("|");

    for (var i = 0; i < elementsToDisplay.length; i++) {

        var elementToDisplay = $(elementsToDisplay[i]);
        var elementToCheck = $(elementsToCheck[i]);

        var checkedValue = elementToCheck.val().toString();

        if (checkedValue === checkedValues[i].toString()) {
            elementToDisplay.show();
        } else {
            elementToDisplay.hide();
        }

    }


    CalculatePrice();

}

function RenderHidden(calculatorGroupOption) {

    var value = calculatorGroupOption.CalculatorGroupOptionValues[0].Id;
    var template = '<input id="input-' + calculatorGroupOption.Id + '" type="hidden" value="' + value + '" />';
    return template;
}



function RenderFileUpload(calculatorGroupOption, required) {
    return ''; //TBC
}


function CalculatePrice() {

    userCalculator = null;
    userCalculator = {};
    userCalculator.Choices = [];
    userCalculator.CalculatorName = calculator.Name;

    ClearWorkings();

    var numberOfCopiesElement = $('#input-' + numberOfCopiesId);


    if (pagesInFileId === undefined || pagesInFileId === null) {

        pagesInFile = 1;

    } else {

        var pagesInFileElement = $('#input-' + pagesInFileId);
        pagesInFile = parseInt(pagesInFileElement.val());

    }

    if (noOfSidesOption === undefined || noOfSidesOption === null) {

        numberOfSides = 1;

    } else {

        var noOfSidesElement = $('#input-' + noOfSidesOption.Id);
        var numberOfSidesId = parseInt(noOfSidesElement.val())
        numberOfSides = noOfSidesOption.CalculatorGroupOptionValues.filter((item) => item.Id === numberOfSidesId)[0].PriceMaterial;

    }

    if (numberOfCopiesElement.is("select")) {

        numberOfCopies = parseInt(numberOfCopiesElement.children("option").filter(":selected").text());

    } else if (numberOfCopiesElement.is("input")) {

        numberOfCopies = parseInt(numberOfCopiesElement.val());

    }

    totalCopiesCount = numberOfCopies * pagesInFile;
    totalPagesCount = totalCopiesCount / numberOfSides;

    WriteWorkingLine('<b>Counts</b>');
    WriteWorkingLine(` - Number Of Copies: ${numberOfCopies}`);
    WriteWorkingLine(` - Pages In File: ${pagesInFile}`);
    WriteWorkingLine(` - Number Of Sides: ${numberOfSides}`);
    WriteWorkingLine(` - Total Copies Count: ${totalCopiesCount}`);
    WriteWorkingLine(` - Total Pages Count: ${totalPagesCount}`);

    AppendUserCalculatorItem('Number of Copies', numberOfCopies);
    AppendUserCalculatorItem('Pages in File', pagesInFile);
    AppendUserCalculatorItem('Number of Sides', numberOfSides);
    AppendUserCalculatorItem('Total Copies', totalCopiesCount);
    AppendUserCalculatorItem('Total Pages', totalPagesCount);


    var totalPaperCosts = 0;
    var totalPaperWeights = 0;
    var totalCopyCosts = 0;
    var totalCopyWeights = 0;
    var totalOneOffCosts = 0;
    var totalOneOffWeights = 0;
    var quantityDiscount = 0;

    var totalCopyCostRatio = 1;
    var totalPaperCostRatio = 1;
    var totalCopyWeightRatio = 1;
    var totalPaperWeightRatio = 1;

    var weightRatio = 1;

    WriteWorkingLine('<b>Workings</b>');

    copyPricedElements = [];
    oneOffPricedElements = [];

    for (var a = 0; a < allElements.length; a++) {

 
        AppendUserCalculatorItem(allElements[a].Name, $('#input-' + allElements[a].Id + ' option:selected').html());

        //Paper Costs
        if (allElements[a].CalculatorGroupPricingTypeId === 10) {

            WriteWorkingLine(`<b>Option: ${allElements[a].Name} (Paper Cost)</b>`);

            var paperElement = $('#input-' + allElements[a].Id);
            var paperElementValueId = parseInt(paperElement.val());

            var calculatorGroupOptionValuepaper = allElements[a].CalculatorGroupOptionValues.filter((item) => item.Id === paperElementValueId)[0];

            var paperCost = calculatorGroupOptionValuepaper.PriceMaterial * totalPagesCount
            var paperWeight = calculatorGroupOptionValuepaper.Weight * totalPagesCount;

            totalPaperCosts += paperCost;
            totalPaperWeights += paperWeight;

            WriteWorkingLine(` - Paper Cost: $${calculatorGroupOptionValuepaper.PriceMaterial} * ${totalPagesCount} = $${paperCost}`);
            WriteWorkingLine(` - Paper Weight: ${calculatorGroupOptionValuepaper.Weight} * ${totalPagesCount} = ${paperWeight}`);

          
        }

        //Copy Costs
        if (allElements[a].CalculatorGroupPricingTypeId === 20) {

            WriteWorkingLine(`<b>Option: ${allElements[a].Name} (Copy Cost)</b>`);

            var copyElement = $('#input-' + allElements[a].Id);
            var copyElementValueId = parseInt(copyElement.val());

            var calculatorGroupOptionValuecopy = allElements[a].CalculatorGroupOptionValues.filter((item) => item.Id === copyElementValueId)[0];

            var copyCost = calculatorGroupOptionValuecopy.PriceMaterial * numberOfCopies
            var copyWeight = calculatorGroupOptionValuecopy.Weight * numberOfCopies;

            totalCopyCosts += copyCost;
            totalCopyWeights += copyWeight;

            WriteWorkingLine(` - Copy Cost: $${calculatorGroupOptionValuecopy.PriceMaterial} * ${numberOfCopies} = $${copyCost}`);
            WriteWorkingLine(` - Copy Weight: ${calculatorGroupOptionValuecopy.Weight} * ${numberOfCopies} = ${copyWeight}`);


        }

        //Total Copy Costs
        if (allElements[a].CalculatorGroupPricingTypeId === 25) {

            WriteWorkingLine(`<b>Option: ${allElements[a].Name} (Total Copy Cost)</b>`);

            var totalCopyElement = $('#input-' + allElements[a].Id);
            var totalCopyElementValueId = parseInt(totalCopyElement.val());

            var calculatorGroupOptionValuetotalCopy = allElements[a].CalculatorGroupOptionValues.filter((item) => item.Id === totalCopyElementValueId)[0];

            var totalCopyCost = calculatorGroupOptionValuetotalCopy.PriceMaterial * totalCopiesCount
            var totalCopyWeight = calculatorGroupOptionValuetotalCopy.Weight * totalCopiesCount;

            totalCopyCosts += totalCopyCost;
            totalCopyWeights += totalCopyWeight;

            WriteWorkingLine(` - Total Copy Cost: $${calculatorGroupOptionValuetotalCopy.PriceMaterial} * ${totalCopiesCount} = $${totalCopyCost}`);
            WriteWorkingLine(` - Total Copy Weight: ${calculatorGroupOptionValuetotalCopy.Weight} * ${totalCopiesCount} = ${totalCopyWeight}`);

        }

        //Ratio Paper
        if (allElements[a].CalculatorGroupPricingTypeId === 40) {

            WriteWorkingLine(`<b>Option: ${allElements[a].Name} (Paper Ratio Cost)</b>`);

            var paperRatioElement = $('#input-' + allElements[a].Id);
            var paperRatioElementValueId = parseInt(paperRatioElement.val());

            var calculatorGroupOptionValuepaperRatio = allElements[a].CalculatorGroupOptionValues.filter((item) => item.Id === paperRatioElementValueId)[0];

            totalPaperCostRatio = CalculateRatio(totalPaperCostRatio, calculatorGroupOptionValuepaperRatio.PriceMaterial);
            totalPaperWeightRatio = CalculateRatio(totalPaperWeightRatio, calculatorGroupOptionValuepaperRatio.Weight);

            //totalPaperCostRatio += calculatorGroupOptionValuepaperRatio.PriceMaterial;
            //totalPaperWeightRatio += calculatorGroupOptionValuepaperRatio.Weight;

            WriteWorkingLine(` - Paper Cost Ratio: ${calculatorGroupOptionValuepaperRatio.PriceMaterial}`);
            WriteWorkingLine(` - Paper Weight Ratio: ${calculatorGroupOptionValuepaperRatio.Weight}`);
        }


        //Ratio Copies
        if (allElements[a].CalculatorGroupPricingTypeId === 50) {

            WriteWorkingLine(`<b>Option: ${allElements[a].Name} (Copy Ratio Cost)</b>`);

            var copyRatioElement = $('#input-' + allElements[a].Id);
            var copyRatioElementValueId = parseInt(copyRatioElement.val());

            var calculatorGroupOptionValuecopyRatio = allElements[a].CalculatorGroupOptionValues.filter((item) => item.Id === copyRatioElementValueId)[0];

            totalCopyCostRatio = CalculateRatio(totalCopyCostRatio, calculatorGroupOptionValuecopyRatio.PriceMaterial);
            totalCopyWeightRatio = CalculateRatio(totalCopyWeightRatio, calculatorGroupOptionValuecopyRatio.Weight);

            //totalCopyCostRatio += calculatorGroupOptionValuecopyRatio.PriceMaterial;
            //totalCopyWeightRatio += calculatorGroupOptionValuecopyRatio.Weight;

            WriteWorkingLine(` - Copy Cost Ratio: ${calculatorGroupOptionValuecopyRatio.PriceMaterial}`);
            WriteWorkingLine(` - Copy Weight Ratio: ${calculatorGroupOptionValuecopyRatio.Weight}`);

        }


        //Ratio Copie & Paper
        if (allElements[a].CalculatorGroupPricingTypeId === 60) {

            WriteWorkingLine(`<b>Option: ${allElements[a].Name} (Copy & Paper Ratio Cost)</b>`);

            var copyPaperRatioElement = $('#input-' + allElements[a].Id);
            var copyPaperRatioElementValueId = parseInt(copyPaperRatioElement.val());

            var calculatorGroupOptionValuecopyPaperRatio = allElements[a].CalculatorGroupOptionValues.filter((item) => item.Id === copyPaperRatioElementValueId)[0];

            totalPaperCostRatio = CalculateRatio(totalPaperCostRatio, calculatorGroupOptionValuecopyPaperRatio.PriceMaterial);
            totalCopyCostRatio = CalculateRatio(totalCopyCostRatio, calculatorGroupOptionValuecopyPaperRatio.PriceCopy);
            totalCopyWeightRatio = CalculateRatio(totalCopyWeightRatio, calculatorGroupOptionValuecopyPaperRatio.Weight);

            //totalPaperCostRatio += calculatorGroupOptionValuecopyPaperRatio.PriceMaterial;
            //totalCopyCostRatio += calculatorGroupOptionValuecopyPaperRatio.PriceCopy;
            //totalCopyWeightRatio += calculatorGroupOptionValuecopyPaperRatio.Weight; //TODO:

            WriteWorkingLine(` - Copy Cost Loading: ${calculatorGroupOptionValuecopyPaperRatio.PriceCopy * 100} %`);
            WriteWorkingLine(` - Paper Cost Loading: ${calculatorGroupOptionValuecopyPaperRatio.PriceMaterial * 100} %`);
            WriteWorkingLine(` - Copy Weight Ratio: ${calculatorGroupOptionValuecopyPaperRatio.Weight}`);

        }

        //Adjust any weight ratios
        if (allElements[a].CalculatorGroupWeightTypeId === 2) {

            WriteWorkingLine(`<b>Option: ${allElements[a].Name} (Weight Ratio Adjustment)</b>`);

            var weightRatioElement = $('#input-' + allElements[a].Id);
            var weightRatioElementValueId = parseInt(weightRatioElement.val());

            var calculatorGroupOptionValueweightRatio = allElements[a].CalculatorGroupOptionValues.filter((item) => item.Id === weightRatioElementValueId)[0];

            weightRatio = CalculateRatio(weightRatio, calculatorGroupOptionValueweightRatio.WeightRatio);

            //weightRatio += calculatorGroupOptionValueweightRatio.WeightRatio

            WriteWorkingLine(` - Weight Ratio Adjustment: ${calculatorGroupOptionValueweightRatio.WeightRatio}`);

        }
    

        //One Off Costs
        if (allElements[a].CalculatorGroupPricingTypeId === 30) {

            WriteWorkingLine(`<b>Option: ${allElements[a].Name} (One Off Cost)</b>`);

            var oneOffElement = $('#input-' + allElements[a].Id);
            var oneOffElementValueId = parseInt(oneOffElement.val());

            var calculatorGroupOptionValueOneOff = allElements[a].CalculatorGroupOptionValues.filter((item) => item.Id === oneOffElementValueId)[0];

            totalOneOffCosts += calculatorGroupOptionValueOneOff.PriceMaterial;
            totalOneOffWeights += calculatorGroupOptionValueOneOff.Weight;

            WriteWorkingLine(` - Off One Cost: $${calculatorGroupOptionValueOneOff.PriceMaterial}`);
            WriteWorkingLine(` - Off One Weight: ${calculatorGroupOptionValueOneOff.Weight}`);

        }
    }



    /* Quantity Discounts */
    for (var pb = 0; pb < calculator.CalculatorPriceBands.length; pb++) {

        var priceBand = calculator.CalculatorPriceBands[pb];

        if (totalCopiesCount >= priceBand.QuantityFrom && totalCopiesCount <= priceBand.QuantityTo) {

            if (priceBand.PercentageAdjustment !== 0) {
                quantityDiscount = priceBand.PercentageAdjustment / 100;
            }

            WriteWorkingLine('<b>Applied Quantity Discount of:</b> ' + priceBand.PercentageAdjustment + '%');

            AppendUserCalculatorItem('Quantity Discount', priceBand.PercentageAdjustment + '%');

            break;
        }



    }

    //Resolve unused ratios
    if (totalCopyCostRatio === 0) totalCopyCostRatio = 1;
    if (totalPaperCostRatio === 0) totalPaperCostRatio = 1;
    if (totalCopyWeightRatio === 0) totalCopyWeightRatio = 1;
    if (totalPaperWeightRatio === 0) totalPaperWeightRatio = 1;

    var subTotalPaperCosts = totalPaperCosts * totalPaperCostRatio;
    var subTotalCopyCosts = totalCopyCosts * totalCopyCostRatio;

    var subTotalPriceBeforeDiscount = subTotalPaperCosts + subTotalCopyCosts;

    AppendUserCalculatorItem('Paper Costs', subTotalPaperCosts);
    AppendUserCalculatorItem('Copy Costs', subTotalCopyCosts);
    AppendUserCalculatorItem('Sub Total Before Discount', subTotalPriceBeforeDiscount);

    console.log('SubTotal Price Before Discount: ' + subTotalPriceBeforeDiscount);

    //Only apply quantity discount to Copy Costs
    subTotalPrice = subTotalPriceBeforeDiscount - (subTotalCopyCosts * quantityDiscount);
    //subTotalPrice = subTotalPriceBeforeDiscount - (subTotalPriceBeforeDiscount * quantityDiscount);

    if (subTotalPrice < calculator.MinimumOrder) {
        WriteWorkingLine('Subtotal: $' + parseFloat(subTotalPrice).toFixed(2) + ' -  Apply Miniumum Order of: ' + parseFloat(calculator.MinimumOrder).toFixed(2));
        subTotalPrice = calculator.MinimumOrder;
    }

    setupPrice = totalOneOffCosts;
    totalPrice = subTotalPrice + setupPrice;

    totalWeight = (totalPaperWeights + totalCopyWeights + totalOneOffWeights) * weightRatio;

    $('#calculator-total-toplabel').html('$' + parseFloat(totalPrice).toFixed(2));

    $('#subtotalprice-label').html('$' + parseFloat(subTotalPrice).toFixed(2));
    $('#setupprice-label').html('$' + parseFloat(setupPrice).toFixed(2));
    $('#totalprice-label').html('$' + parseFloat(totalPrice).toFixed(2));

    $('#calculator-total-toplabel').html('$' + parseFloat(totalPrice).toFixed(2));


    WriteWorkingLine('<b>Totals</b>');
    WriteWorkingLine('Paper Price: $' + parseFloat(subTotalPaperCosts).toFixed(2));
    WriteWorkingLine('Copy Price: $' + parseFloat(subTotalCopyCosts).toFixed(2));
    WriteWorkingLine('One Off Price: $' + parseFloat(totalOneOffCosts).toFixed(2));

    WriteWorkingLine('Subtotal Price: $' + parseFloat(subTotalPrice).toFixed(2));
    WriteWorkingLine('Setup Price: $' + parseFloat(setupPrice).toFixed(2))
    WriteWorkingLine('Total Price: $' + parseFloat(totalPrice).toFixed(2));
    WriteWorkingLine('Total Weight: ' + parseFloat(totalWeight).toFixed(2));

    AppendUserCalculatorItem('(Admin) Discount', '$' + (subTotalPriceBeforeDiscount * quantityDiscount).toFixed(2));
    AppendUserCalculatorItem('(Admin) SubTotal', '$' + parseFloat(subTotalPrice).toFixed(2));
    AppendUserCalculatorItem('(Admin) Setup ', '$' + parseFloat(setupPrice).toFixed(2));
    AppendUserCalculatorItem('(Admin) Total', '$' + parseFloat(totalPrice).toFixed(2));
    AppendUserCalculatorItem('(Admin) Total Weight: ' +  parseFloat(totalWeight).toFixed(2));

    userCalculator.SubTotal = subTotalPrice;
    userCalculator.Setup = setupPrice;
    userCalculator.Total = totalPrice;
    userCalculator.Weight = totalWeight

}

function CalculateRatio(ratioToAmend, newValue) {

    if (newValue === 0) newValue = 1;
    ratioToAmend = ratioToAmend * newValue;
    return ratioToAmend;

}

function ClearWorkings() {

    $('#workings').html('');

}

function WriteWorkingLine(text) {

    var workings = $('#workings').html();
    $('#workings').html(workings  + text + '<br />');

}

function AppendUserCalculatorItem(name, value) {

    if (!value) return;

    userCalculator.Choices.push({
        Name: name,
        Value: value.toString()
    });

}