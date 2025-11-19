$(function () {
    $("#FormSeguridad").validate({
        rules: {
            Contrasena: {
                required: true,
                maxlength: 10
            },
            ContrasenaConfirmar: {
                required: true,
                equalTo: "#Contrasena",
                maxlength: 10
            }
        },
        messages: {
            Contrasenna: {
                required: "* Requerido",
                maxlength: "* Máximo 10 caracteres"
            },
            ContrasenaConfirmar: {
                required: "* Requerido",
                equalTo: "* La confirmación no coincide",
                maxlength: "* Máximo 10 caracteres"
            }
        }
    });
});