$(document).ready(() => {
    let myChart;

  
    const makeAjaxCall = (url, method, data = null, successCallback, errorCallback) => {
        $.ajax({
            url,
            method,
            contentType: "application/json",
            data: data ? JSON.stringify(data) : undefined,
            success: successCallback,
            error: errorCallback
        });
    };

    $("#dbForm").on("submit", (event) => {
        event.preventDefault();

        const connectionData = {
            server: $("#server").val(),
            user: $("#user").val(),
            pass: $("#pass").val(),
            database: $("#db").val()
        };

        const connectionMessage = $("#connectionMessage");

        makeAjaxCall(
            "https://localhost:7205/api/Data/verifyConnection",
            "POST",
            connectionData,
            (data) => {
                if (data.success) {
                    connectionMessage.text("Baglanildi.").show();
                    $("#columnsSection").show();
                    setTimeout(() => connectionMessage.fadeOut(), 3000);
                    fetchProcedures();
                    fetchViews();
                } else {
                    connectionMessage.text("Bilgilerinizde bir hata var, lütfen düzeltin.").show();
                }
            },
            (error) => {
                connectionMessage.text("Hata: " + error.message).css("color", "red").show();
            }
        );
    });

    const fetchProcedures = () => {
        makeAjaxCall(
            "https://localhost:7205/api/Data/getProcedures",
            "GET",
            null,
            (procedures) => {
                const comboBox = $("#proceduresComboBox");
                comboBox.empty();
                procedures.forEach((procedure) => {
                    comboBox.append(new Option(procedure, procedure));
                });
                $("#proceduresComboBox, #executeProcedureButton").show();
            },
            (error) => console.error("Prosedürler alýnýrken bir hata oluþtu:", error)
        );
    };

 
    const fetchViews = () => {
        makeAjaxCall(
            "https://localhost:7205/api/Data/getViews",
            "GET",
            null,
            (views) => {
                const comboBox = $("#viewsComboBox");
                comboBox.empty();
                views.forEach((view) => {
                    comboBox.append(new Option(view, view));
                });
                $("#procedureId, #viewId").show();
            },
            (error) => console.error("View alýnýrken bir hata oluþtu:", error)
        );
    };


    $("#executeProcedureButton").on("click", () => {
        const selectedProcedure = $("#proceduresComboBox").val();
        if (!selectedProcedure) {
            alert("Lütfen bir prosedür seçin.");
            return;
        }
        if (myChart) {
            myChart.destroy();
        }
        executeStoredProcedureOrView("https://localhost:7205/api/Data/executeProcedure", selectedProcedure);
    });

    $("#executeViewButton").on("click", () => {
        const selectedView = $("#viewsComboBox").val();
        if (!selectedView) {
            alert("Lütfen bir view seçin.");
            return;
        }
        if (myChart) {
            myChart.destroy();
        }
        executeStoredProcedureOrView("https://localhost:7205/api/Data/executeViews", selectedView);
    });

    const executeStoredProcedureOrView = (url, selectedItem) => {
        makeAjaxCall(
            url,
            "POST",
            selectedItem,
            (data) => {
                if (data.length > 0) {
                    valuesArray = data.map(item => ({ x: item.xValue, y: item.yValue }));
                    $("#graph, #dataMessage").show();
                    $("#dataMessage").text("Veriler baþarýyla toplandý. Þimdi grafik oluþturabilirsiniz.").show();
                    setTimeout(() => $("#dataMessage").fadeOut(), 3000);
                } else {
                    $("#dataMessage").text("Veri bulunamadý.").css("color", "red").show();
                }
            },
            (error) => {
                console.error("Hata:", error);
                alert("Stored procedure veya view çalýþtýrýlýrken bir hata oluþtu.");
            }
        );
    };
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
                text: "Dinamik Cizgi Grafigi"
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
                text: "Dinamik Cubuk Grafigi"
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
        $("#viewId").hide();
        $("#procedureId").hide();
    });
});
