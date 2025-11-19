
function ConsultarNombre() {

    clearTimeout(timer);

    timer = setTimeout(() => {

        let identificacion = $("#Identificacion").val();
        $("#NombreCompleto").val("");

        if (identificacion.length >= 9) {

            $.ajax({
                type: 'GET',
                url: 'https://apis.gometa.org/cedulas/' + identificacion,
                dataType: 'json',
                success: function (data) {
                    if (data.resultcount > 0 && data.nombre) {
                        $("#NombreCompleto").val(data.nombre);
                    }
                },
                error: function () {
                    console.warn("No se pudo consultar el nombre.");
                }
            });
        }

    });
}


$(function () {

    $("#FormPerfil").validate({
        rules: {
            NombreCompleto: {
                required: true
            },
            CorreoElectronico: {
                required: true,
                email: true
            },
            Contrasena: {
                required: true,
                minlength: 6
            },
            TipoIdentificacion: {
                required: true
            },
            Identificacion: {
                required: true,
                minlength: 9
            },
            FechaNacimiento: {
                required: true
            },
            Telefono: {
                required: true,
                minlength: 8
            },
            Direccion: {
                required: true
            }
        },
        messages: {
            NombreCompleto: {
                required: "* Requerido"
            },
            CorreoElectronico: {
                required: "* Requerido",
                email: "* Ingrese un correo válido"
            },
            Contrasena: {
                required: "* Requerido",
                minlength: "* Mínimo 6 caracteres"
            },
            TipoIdentificacion: {
                required: "* Seleccione una opción"
            },
            Identificacion: {
                required: "* Requerido",
                minlength: "* Debe tener al menos 9 dígitos"
            },
            FechaNacimiento: {
                required: "* Requerido"
            },
            Telefono: {
                required: "* Requerido",
                minlength: "* Debe tener al menos 8 dígitos"
            },
            Direccion: {
                required: "* Requerido"
            }
        }
    });

});
