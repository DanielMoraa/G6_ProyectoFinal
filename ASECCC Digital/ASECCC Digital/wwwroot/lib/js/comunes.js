function ConsultarNombre() {

    let identificacion = $("#Identificacion").val();
    $("#NombreCompleto").val("");

    if (identificacion.length >= 9) {

        $.ajax({
            type: 'GET',
            url: 'https://apis.gometa.org/cedulas/' + identificacion,
            dataType: 'json',
            success: function (data) {
                if (data.resultcount > 0) {
                    $("#NombreCompleto").val(data.nombre);
                }
            }
        });

    }
}