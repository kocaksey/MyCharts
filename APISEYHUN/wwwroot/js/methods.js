
let myChart = null;

$(document).ready(function () {
    const apiUrl = window.location.origin;

    $("#enableJoin").on("change", function () {
        const isChecked = $(this).is(":checked");
        $("#joinTableSelect, #joinColumnSelect, #displayColumnSelect").prop("disabled", !isChecked);
    });

    $("#connectBtn").on("click", function () {
        const connectionRequest = {
            DatabaseType: $("#dbType").val(),
            ServerName: $("#server").val(),
            DatabaseName: $("#database").val(),
            Username: $("#userId").val(),
            Password: $("#password").val()
        };

        $.ajax({
            url: `${apiUrl}/api/ConnectionString/SetConnection`,
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(connectionRequest),
            success: function (data) {
                $("#message").text(data.message);
                if (data.success) {
                    fetchTables();
                }
            },
            error: function (error) {
                $("#message").text(`Bağlantı hatası: ${error.responseJSON.message}`);
            }
        });
    });

    function fetchTables() {
        $.ajax({
            url: `${apiUrl}/api/Data/GetTables`,
            method: 'GET',
            success: function (tables) {
                const tableSelect = $("#tableSelect");
                tableSelect.empty().append('<option value="">Tablo Seçin</option>');
                tables.forEach(function (table) {
                    tableSelect.append(`<option value="${table}">${table}</option>`);
                });
            },
            error: function (error) {
                console.error('Error fetching tables:', error);
            }
        });
    }

    $("#tableSelect").on("change", function () {
        const table = $(this).val();
        if (!table) {
            alert("Lütfen bir tablo seçin.");
            return;
        }

        $.ajax({
            url: `${apiUrl}/api/Data/GetColumns?tableName=${table}`,
            method: 'GET',
            success: function (columns) {
                const columnSelect = $("#columnSelect");
                columnSelect.empty().append('<option value="">Sütun Seçin</option>');
                columns.forEach(function (column) {
                    columnSelect.append(`<option value="${column}">${column}</option>`);
                });
            },
            error: function (error) {
                console.error('Error fetching columns:', error);
            }
        });
    });

    $("#getChartBtn").on("click", function () {
        const isJoinEnabled = $("#enableJoin").is(":checked");

        if (isJoinEnabled) {
            fetchColumnValueCountsWithJoin();
        } else {
            fetchColumnValueCounts();
        }
    });

    $("#connectBtn").on("click", function () {
        const connectionRequest = {
            DatabaseType: $("#dbType").val(),
            ServerName: $("#server").val(),
            DatabaseName: $("#database").val(),
            Username: $("#userId").val(),
            Password: $("#password").val()
        };

        $.ajax({
            url: `${apiUrl}/api/ConnectionString/SetConnection`,
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(connectionRequest),
            success: function (data) {
                $("#message").removeClass('text-danger').addClass('text-success').text(data.message);
                if (data.success) {
                    fetchTables();
                }
            },
            error: function (error) {
                $("#message").removeClass('text-success').addClass('text-danger').text(`Bağlantı hatası: ${error.responseJSON.message}`);
            }
        });
    });

    function fetchTables() {
        $.ajax({
            url: `${apiUrl}/api/Data/GetTables`,
            method: 'GET',
            success: function (tables) {
                const tableSelect = $("#tableSelect");
                tableSelect.empty().append('<option value="">Tablo Seçin</option>');
                tables.forEach(function (table) {
                    tableSelect.append(`<option value="${table}">${table}</option>`);
                });
            },
            error: function (error) {
                console.error('Error fetching tables:', error);
            }
        });
    }

    function fetchColumnValueCountsWithJoin() {
        const table = $("#tableSelect").val();
        const column = $("#columnSelect").val();
        const joinTable = $("#joinTableSelect").val();
        const joinColumn = $("#joinColumnSelect").val();
        const displayColumn = $("#displayColumnSelect").val();
        const chartType = $("#chartTypeSelect").val();

        if (!table || !column || !joinTable || !joinColumn || !displayColumn) {
            alert("Lütfen gerekli tüm alanları doldurun.");
            return;
        }

        $.ajax({
            url: `${apiUrl}/api/Data/GetColumnValueCountsWithJoin?tableName=${table}&columnName=${column}&joinTable=${joinTable}&joinColumn=${joinColumn}&displayColumn=${displayColumn}`,
            method: 'GET',
            success: function (data) {
                const labels = data.map(item => item[displayColumn]);
                const counts = data.map(item => item.Count);
                drawChart(chartType, labels, counts, displayColumn);
            },
            error: function (error) {
                console.error('Error fetching column value counts with join:', error);
            }
        });
    }

    function fetchColumnValueCounts() {
        const table = $("#tableSelect").val();
        const column = $("#columnSelect").val();
        const chartType = $("#chartTypeSelect").val();

        if (!table || !column) {
            alert("Lütfen tabloyu ve sütunu seçin.");
            return;
        }

        $.ajax({
            url: `${apiUrl}/api/Data/GetColumnValueCounts?tableName=${table}&columnName=${column}`,
            method: 'GET',
            success: function (data) {
                const labels = data.map(item => item[column]);
                const counts = data.map(item => item.Count);
                drawChart(chartType, labels, counts, column);
            },
            error: function (error) {
                console.error('Error fetching column value counts:', error);
            }
        });
    }

    function drawChart(chartType, labels, counts, displayColumn) {
        const ctx = document.getElementById('myChart').getContext('2d');

        if (myChart) {
            myChart.destroy();
        }

        myChart = new Chart(ctx, {
            type: chartType,
            data: {
                labels: labels,
                datasets: [{
                    label: `Değer - ${displayColumn}`,
                    data: counts,
                    backgroundColor: chartType === 'radar' ? 'rgba(54, 162, 235, 0.2)' : chartType === 'bar' ? 'rgba(54, 162, 235, 0.2)' : 'rgba(54, 162, 235, 0)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 1,
                    fill: chartType === 'line' || chartType === 'radar' ? true : false
                }]
            },
            options: {
                scales: chartType === 'radar' ? {} : {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }
});
