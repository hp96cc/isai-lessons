 (function (factory) {
            if (typeof define === 'function' && define.amd) {
                // AMD
                define(['jquery', 'datatables', 'datatables-editor'], factory);
            }
            else if (typeof exports === 'object') {
                // Node / CommonJS
                module.exports = function ($, dt) {
                    if (!$) { $ = require('jquery'); }
                    factory($, dt || $.fn.dataTable || require('datatables'));
                };
            }
            else if (jQuery) {
                // Browser standard
                factory(jQuery, jQuery.fn.dataTable);
            }
        }(function ($, DataTable) {
            'use strict';


            if (!DataTable.ext.editorFields) {
                DataTable.ext.editorFields = {};
            }

            var _fieldTypes = DataTable.Editor ?
                DataTable.Editor.fieldTypes :
                DataTable.ext.editorFields;


            _fieldTypes.chosen = {
                "_addOptions": function (conf, opts) {
                    var elOpts = conf._input[0].options;

                    elOpts.length = 0;

                    if (opts) {
                        DataTable.Editor.pairs(opts, conf.optionsPair, function (val, label, i) {
                            elOpts[i] = new Option(label, val);
                        });
                    }
                },

                create: function (conf) {
                    conf._input = $('<select/>')
                        .attr($.extend({
                            id: conf.id
                        }, conf.attr || {}));

                    _fieldTypes.chosen._addOptions(conf, conf.options || conf.ipOpts);

                    // On open, need to have the instance update now that it is in the DOM
                    this.on('open.chosen-' + conf.id, function () {
                        conf._input.chosen($.extend({}, conf.opts, { width: '100%' }));
                    });

                    return conf._input[0];
                },

                get: function (conf) {
                    return conf._input.val();
                },

                set: function (conf, val) {
                    conf._input.val(val).trigger('chosen:updated');
                },

                enable: function (conf) {
                    conf._input.attr('disabled', false).trigger('chosen:updated');
                    $(conf._input).removeClass('disabled');
                },

                disable: function (conf) {
                    conf._input.attr('disabled', true).trigger('chosen:updated');
                    $(conf._input).addClass('disabled');
                },

                update: function (conf, options) {
                    _fieldTypes.chosen._addOptions(conf, options);
                    conf._input.trigger('chosen:updated');
                }
            };


        }));