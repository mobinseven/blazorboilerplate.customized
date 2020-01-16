var table = null;
moment.loadPersian();
window.ConvertAllTimeCells = function () {
    var cells = [];
    cells = document.getElementsByClassName('datetime');
    for (var i = 0; i < cells.length; i++) {
        var item = cells.item(i);
        item.innerHTML = moment(item.dataset.order, "X").format('dddd jD jMMMM  jYY | HH:mm');
    }
}
window.ConvertAllTimeCellsFromNow = function () {
    var cells = [];
    cells = document.getElementsByClassName('datetime');
    for (var i = 0; i < cells.length; i++) {
        var item = cells.item(i);
        item.innerHTML = moment(item.dataset.order, "X").fromNow();
    }
}
window.MakeDataTable = function () {
    $('.' + scrollerClassName).bind('scroll', function (e) { //TODO: scrollerClassName: Defined in ScrollPosition.js
        table.fixedHeader.adjust();
    });

    table = $('.table').DataTable({
        autoWidth: false,
        fixedHeader: true,
        paging: false,
        responsive: {
            details: {
                type: 'column',
                target: 'tr',
                renderer: function (api, rowIdx, columns) {
                    var data = $.map(columns, function (col, i) {
                        return col.hidden ?
                            '<div data-dt-row="' + col.rowIndex + '" data-dt-column="' + col.columnIndex + '">' +
                            col.data +
                            '</div>' :
                            '';
                    }).join('');
                    return data ?
                        $('<div/>')
                            //.attr('style', 'width:50%')
                            .append(data) :
                        false;
                }
            }
        },
        language: {
            searchPlaceholder: String.fromCharCode(0xf002),
            "decimal": "",
            "emptyTable": "بدون رده",
            "info": "_START_ تا _END_ از _TOTAL_ رده",
            "infoEmpty": " 0 تا 0 از 0 رده",
            "infoFiltered": "(پالایش شده از _MAX_ رده)",
            "infoPostFix": "",
            "thousands": ",",
            "lengthMenu": "نمایش _MENU_",
            "loadingRecords": "دریافت داده‌ها ...",
            "processing": "پردازش داده‌ها ...",
            "search": "",
            "zeroRecords": "چیزی یافت نشد",
            "paginate": {
                "first": "نخستین",
                "last": "واپسین",
                "next": "پسین",
                "previous": "پیشین"
            }
        }
    });

    $(".DatePicker.from").persianDatepicker({
        autoClose: true,
        initialValueType: 'persian',
        format: 'YY/MM/DD',
        onSelect: function (unix) {
            table.minDateFilter = unix / 1000;
            table.draw();
        }
    });
    $(".DatePicker.to").persianDatepicker({
        autoClose: true,
        initialValueType: 'persian',
        format: 'YY/MM/DD',
        onSelect: function (unix) {
            table.maxDateFilter = unix / 1000;
            table.draw();
        }
    });
    $(".DatePickerClear.from").click(function () {
        $(".DatePicker.from").val("");
        table.minDateFilter = undefined;
        table.draw();
    });
    $(".DatePickerClear.to").click(function () {
        $(".DatePicker.to").val("");
        table.maxDateFilter = undefined;
        table.draw();
    });
    $.fn.dataTableExt.afnFiltering.push(function (oSettings, aData, iDataIndex) {
        var Date = aData[0];//TODO: Date must be the first column ALWAYS, in which we have data-search="1456950600000"
        //if (typeof aData._date == 'undefined') {
        //    aData._date = new Date(aData[0]).getTime();
        //}
        if (typeof table.minDateFilter != "undefined") {
            if (Date < table.minDateFilter) {
                return false;
            }
        }

        if (typeof table.maxDateFilter != "undefined") {
            if (Date > table.maxDateFilter) {
                return false;
            }
        }

        return true;
    });
}
window.DestroyDataTable = function () {
    table.destroy();
}