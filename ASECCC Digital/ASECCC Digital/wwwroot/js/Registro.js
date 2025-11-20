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

    identificacion.addEventListener("input", function () {
        if (!tipoIdentificacion.value) {
            identificacion.value = "";
            setError(tipoIdentificacion, "Debe seleccionar el tipo de identificación primero.");
        } else {
            clearError(tipoIdentificacion);
        }
    });

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
        e.preventDefault();
        let valido = true;

        clearError(correo);
        clearError(contrasena);
        clearError(identificacion);
        clearError(fechaNacimiento);
        clearError(telefono);
        clearError(direccion);
        clearError(tipoIdentificacion);

        if (!tipoIdentificacion.value) {
            setError(tipoIdentificacion, "Seleccione el tipo de identificación.");
            valido = false;
        }

        let cedulaLimpia = identificacion.value.replace(/\D/g, "");

        if (tipoIdentificacion.value === "Nacional" && cedulaLimpia.length !== 9) {
            setError(identificacion, "La cédula nacional debe contener 9 dígitos.");
            valido = false;
        }

        if (tipoIdentificacion.value === "DIMEX" && cedulaLimpia.length !== 12) {
            setError(identificacion, "La cédula DIMEX debe contener 12 dígitos.");
            valido = false;
        }

        if (!correo.value.includes("@")) {
            setError(correo, "El correo debe contener '@'.");
            valido = false;
        }

        if (contrasena.value.length < 6) {
            setError(contrasena, "La contraseña debe tener al menos 6 caracteres.");
            valido = false;
        }

        if (!fechaNacimiento.value) {
            setError(fechaNacimiento, "Debe seleccionar su fecha de nacimiento.");
            valido = false;
        }

        if (!/^\d{8}$/.test(telefono.value)) {
            setError(telefono, "El teléfono debe ser numérico y contener 8 dígitos.");
            valido = false;
        }

        if (direccion.value.trim() === "") {
            setError(direccion, "La dirección es obligatoria.");
            valido = false;
        }

        if (!valido) {
            Swal.fire("Atención", "Por favor corrija los errores en el formulario.", "warning");
            return;
        }

        registrarUsuario();
    });

    function registrarUsuario() {
        const formData = {
            TipoIdentificacion: tipoIdentificacion.value,
            Identificacion: identificacion.value,
            NombreCompleto: nombreCompleto.value,
            CorreoElectronico: correo.value,
            Contrasena: contrasena.value,
            FechaNacimiento: fechaNacimiento.value,
            Telefono: telefono.value,
            Direccion: direccion.value
        };

        fetch("/Home/Registro", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(formData)
        })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    Swal.fire({
                        title: "Éxito",
                        text: "Registro completado correctamente",
                        icon: "success",
                        confirmButtonText: "Aceptar"
                    }).then(() => {
                        // Redirigir al login después del registro exitoso
                        window.location.href = "/Home/Login";
                    });
                } else {
                    Swal.fire("Error", data.message || "No se pudo completar el registro", "error");
                }
            })
            .catch(error => {
                console.error("Error:", error);
                Swal.fire("Error", "Ocurrió un error al procesar el registro", "error");
            });
    }

});