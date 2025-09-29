function ConsultarNombre() {
    //let identificacion = document.getElementById("txtIdentificacion").value;
    let identificacion = $("#txtIdentificacion").val();

    document.getElementById("txtNombre").value = "";
    //$("#txtNombre").val("");

    if (identificacion.length >= 9) {
        $.ajax({
            url: 'https://apis.gometa.org/cedulas/' + identificacion,
            method: 'GET',
            success: function (data) {
                document.getElementById("txtNombre").value = data.nombre;
                //$("#txtNombre").val(data.nombre);
            }
        });
    }
}