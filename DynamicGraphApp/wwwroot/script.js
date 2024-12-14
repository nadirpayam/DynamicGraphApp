$(document).ready(() => {
    let myChart;

    $("#dbForm").on("submit", (event) => {
        event.preventDefault();

        let server = $("#server").val();
        let user = $("#user").val();
        let pass = $("#pass").val();
        let db = $("#db").val();
        let connectionMessage = $("#connectionMessage");

        $.ajax({
            url: "https://localhost:7205/api/Data/verifyConnection",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                server: server,
                user: user,
                pass: pass,
                database: db
            }),
            success: (data) => {

                if (data.success) {
                    $("#columnsSection").show();
                    connectionMessage.text("Baglanildi.").show();
                    setTimeout(() => {
                        connectionMessage.fadeOut();
                    }, 3000);
                } else {
                    connectionMessage.text("Bilgilerinizde bir hata var duzeltin lutfen.").show();
                }
            },
            error: (error) => {
                connectionMessage.text("Hata: " + error.message).css("color", "red").show();
            }

        });
    });

    let valuesArray = [];

    $("#fetchDataButton").on("click", () => {
        let tableName = $("#tableName").val();
        let xColumn = $("#xColumn").val();
        let yColumn = $("#yColumn").val();
        let dataMessage = $("#dataMessage");

        $.ajax({
            url: "https://localhost:7205/api/data/GetData",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                tableName: tableName,
                xColumn: xColumn,
                yColumn: yColumn
            }),
            success: (data) => {
                if (data.length > 0) {
                    valuesArray = data.map(item => ({
                        x: item.xValue,
                        y: item.yValue
                    }));
                    $("#graph").show();
                    $("#dataMessage").show();
                    dataMessage.text("Veriler basariyla toplandi. Simdi grafik olusturabilirsiniz.").show();
                    setTimeout(() => {
                        dataMessage.fadeOut();
                    }, 3000);
                } else {
                    dataMessage.text("Veri bulunamadi.").css("color", "red").show();
                }
            },
            error: (error) => {
                dataMessage.text("Hata: " + error.message).show();
            }
        });
    });

    const getValues = () => ({
        xValues: valuesArray.map(item => item.x),
        yValues: valuesArray.map(item => item.y)
    });

    const createChart = (type, data, options) => {
        if (myChart) {
            myChart.destroy();
        }
        const ctx = document.getElementById('myChart').getContext('2d');
        myChart = new Chart(ctx, {
            type,
            data,
            options
        });
    };

    const createLineChart = () => {
        const { xValues, yValues } = getValues();

        createChart("line", {
            labels: xValues,
            datasets: [{
                fill: false,
                lineTension: 0,
                backgroundColor: "rgba(0,0,255,1.0)",
                borderColor: "rgba(0,0,255,0.1)",
                data: yValues
            }]
        }, {
            legend: { display: false },
            title: {
                display: true,
                text: "Dinamik Çizgi Grafigi"
            }
        });
    };

    const createBarChart = () => {
        const { xValues, yValues } = getValues();

        createChart("bar", {
            labels: xValues,
            datasets: [{
                backgroundColor: ["red", "green", "blue", "orange", "brown"],
                data: yValues
            }]
        }, {
            legend: { display: false },
            title: {
                display: true,
                text: "Dinamik Çubuk Grafigi"
            }
        });
    };

    const createDoughnutCharts = () => {
        const { xValues, yValues } = getValues();
        const barColors = [
            "#b91d47",
            "#00aba9",
            "#2b5797",
            "#e8c3b9",
            "#1e7145"
        ];

        createChart("doughnut", {
            labels: xValues,
            datasets: [{
                backgroundColor: barColors,
                data: yValues
            }]
        }, {
            title: {
                display: true,
                text: "Dinamik Halka Grafigi"
            }
        });
    };

    const pieCharts = () => {
        const { xValues, yValues } = getValues();
        const barColors = [
            "#b91d47",
            "#00aba9",
            "#2b5797",
            "#e8c3b9",
            "#1e7145"
        ];

        createChart("pie", {
            labels: xValues,
            datasets: [{
                backgroundColor: barColors,
                data: yValues
            }]
        }, {
            title: {
                display: true,
                text: "Dinamik Pasta Grafigi"
            }
        });
    };

    $("#lineChartButton").on("click", createLineChart);
    $("#barChartButton").on("click", createBarChart);
    $("#doughnutChartButton").on("click", createDoughnutCharts);
    $("#pieChartButton").on("click", pieCharts);

    $("#resetButton").on("click", () => {
        if (myChart) {
            myChart.destroy();
        }
        $("#graph").hide();
        $("#server").val("");
        $("#user").val("");
        $("#pass").val("");
        $("#db").val("");
        $("#columnsSection").hide();

        $("#tableName").val("");
        $("#xColumn").val("");
        $("#yColumn").val("");
    });
});
