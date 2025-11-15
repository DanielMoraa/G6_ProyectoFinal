document.addEventListener("DOMContentLoaded", function () {

    const form = document.getElementById("FormRegistro");
    const tipoIdentificacion = document.getElementById("TipoIdentificacion");
    const identificacion = document.getElementById("Identificacion");
    const correo = document.getElementById("CorreoElectronico");
    const contrasena = document.getElementById("Contrasena");
    const fechaNacimiento = document.getElementById("FechaNacimiento");
    const telefono = document.getElementById("Telefono");
    const direccion = document.getElementById("Direccion");
    const nombreCompleto = document.getElementById("NombreCompleto");

    function setError(input, msg) {
        input.classList.add("is-invalid");
        let error = input.parentElement.querySelector(".invalid-feedback");
        if (!error) {
            error = document.createElement("div");
            error.classList.add("invalid-feedback");
            input.parentElement.appendChild(error);
        }
        error.innerText = msg;
    }

    function clearError(input) {
        input.classList.remove("is-invalid");
    }

    // No permitir escribir identificacion sin seleccionar tipo
    identificacion.addEventListener("input", function () {
        if (!tipoIdentificacion.value) {
            identificacion.value = "";
            setError(tipoIdentificacion, "Debe seleccionar el tipo de identificación primero.");
        } else {
            clearError(tipoIdentificacion);
        }
    });

    // Formato de cédula x-xxxx-xxxx
    identificacion.addEventListener("keyup", function () {
        let valor = identificacion.value.replace(/\D/g, "");

        if (tipoIdentificacion.value === "Nacional") {
            if (valor.length > 9) valor = valor.slice(0, 9);
            if (valor.length > 1 && valor.length <= 5) {
                valor = valor.replace(/(\d{1})(\d{1,4})/, "$1-$2");
            } else if (valor.length > 5) {
                valor = valor.replace(/(\d{1})(\d{4})(\d{1,4})/, "$1-$2-$3");
            }
        }

        if (tipoIdentificacion.value === "DIMEX") {
            if (valor.length > 12) valor = valor.slice(0, 12);
        }

        identificacion.value = valor;
    });

    form.addEventListener("submit", function (e) {
        let valido = true;

        clearError(correo);
        clearError(contrasena);
        clearError(identificacion);
        clearError(fechaNacimiento);
        clearError(telefono);
        clearError(direccion);

        // Correo
        if (!correo.value.includes("@")) {
            setError(correo, "El correo debe contener '@'.");
            valido = false;
        }

        // Contraseña
        if (contrasena.value.length < 6) {
            setError(contrasena, "La contraseña debe tener al menos 6 caracteres.");
            valido = false;
        }

        // Tipo identificación seleccionado
        if (!tipoIdentificacion.value) {
            setError(tipoIdentificacion, "Seleccione el tipo de identificación.");
            valido = false;
        }

        // Validar cédula
        let cedulaLimpia = identificacion.value.replace(/\D/g, "");

        if (tipoIdentificacion.value === "Nacional" && cedulaLimpia.length !== 9) {
            setError(identificacion, "La cédula nacional debe contener 9 dígitos.");
            valido = false;
        }

        if (tipoIdentificacion.value === "DIMEX" && cedulaLimpia.length !== 12) {
            setError(identificacion, "La cédula DIMEX debe contener 12 dígitos.");
            valido = false;
        }

        // Fecha nacimiento
        if (!fechaNacimiento.value) {
            setError(fechaNacimiento, "Debe seleccionar su fecha de nacimiento.");
            valido = false;
        }

        // Teléfono
        if (!/^\d{8}$/.test(telefono.value)) {
            setError(telefono, "El teléfono debe ser numérico y contener 8 dígitos.");
            valido = false;
        }

        // Dirección
        if (direccion.value.trim() === "") {
            setError(direccion, "La dirección es obligatoria.");
            valido = false;
        }

        if (!valido) e.preventDefault();
    });

});
