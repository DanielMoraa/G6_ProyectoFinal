document.addEventListener('DOMContentLoaded', function () {
    const loginForm = document.getElementById('FormInicioSesion');
    const tipoIdentificacion = document.getElementById('tipoIdentificacion');
    const identificacion = document.getElementById('Identificacion');
    const contrasenna = document.getElementById('Contrasenna');

    function mostrarError(input, mensaje) {
        removerError(input);

        input.classList.add('is-invalid');

        const errorDiv = document.createElement('div');
        errorDiv.className = 'invalid-feedback';
        errorDiv.textContent = mensaje;
        input.parentElement.appendChild(errorDiv);
    }

    function removerError(input) {
        input.classList.remove('is-invalid');
        const errorDiv = input.parentElement.querySelector('.invalid-feedback');
        if (errorDiv) {
            errorDiv.remove();
        }
    }
    function validarCedula(cedula) {
        cedula = cedula.replace(/[\s-]/g, '');

        if (cedula.length !== 9) {
            return false;
        }

        if (!/^\d+$/.test(cedula)) {
            return false;
        }

        return true;
    }

    function validarDIMEX(dimex) {
        dimex = dimex.replace(/[\s-]/g, '');

        if (dimex.length < 11 || dimex.length > 12) {
            return false;
        }

        if (!/^\d+$/.test(dimex)) {
            return false;
        }

        return true;
    }

    identificacion.addEventListener('input', function () {

        const tipo = tipoIdentificacion.value;

        let valor = this.value.replace(/[^\d]/g, '');

        if (tipo === 'Cedula') {
            if (valor.length > 9) valor = valor.slice(0, 9);

            if (valor.length >= 1 && valor.length <= 9) {
                valor = valor
                    .replace(/^(\d{1})(\d{4})(\d{0,4})$/, function (_, p1, p2, p3) {
                        return p3 ? `${p1}-${p2}-${p3}` : `${p1}-${p2}`;
                    });
            }

        } else if (tipo === 'DIMEX') {
            if (valor.length > 12) valor = valor.slice(0, 12);
        }

        this.value = valor;

        if (this.classList.contains('is-invalid')) {
            removerError(this);
        }
    });

    identificacion.addEventListener('blur', function () {
        const tipo = tipoIdentificacion.value;
        const valor = this.value.trim();

        if (valor === '') {
            return; 
        }

        if (tipo === 'Cedula') {
            if (!validarCedula(valor)) {
                mostrarError(this, 'La cédula debe tener 9 dígitos numéricos');
            }
        } else if (tipo === 'DIMEX') {
            if (!validarDIMEX(valor)) {
                mostrarError(this, 'El DIMEX debe tener entre 11 y 12 dígitos numéricos');
            }
        }
    });

    tipoIdentificacion.addEventListener('change', function () {
        if (identificacion.value.trim() !== '') {
            removerError(identificacion);
            identificacion.dispatchEvent(new Event('blur'));
        }
    });

    loginForm.addEventListener('submit', function (e) {
        let isValid = true;

        removerError(identificacion);
        removerError(contrasenna);

        const tipo = tipoIdentificacion.value;
        const numeroId = identificacion.value.trim();

        if (numeroId === '') {
            mostrarError(identificacion, 'El número de identificación es obligatorio');
            isValid = false;
        } else {
            if (tipo === 'Cedula') {
                if (!validarCedula(numeroId)) {
                    mostrarError(identificacion, 'La cédula debe tener 9 dígitos numéricos');
                    isValid = false;
                }
            } else if (tipo === 'DIMEX') {
                if (!validarDIMEX(numeroId)) {
                    mostrarError(identificacion, 'El DIMEX debe tener entre 11 y 12 dígitos numéricos');
                    isValid = false;
                }
            }
        }

        const password = contrasenna.value.trim();
        if (password === '') {
            mostrarError(contrasenna, 'La contraseña es obligatoria');
            isValid = false;
        } else if (password.length < 6) {
            mostrarError(contrasenna, 'La contraseña debe tener al menos 6 caracteres');
            isValid = false;
        }

        if (!isValid) {
            e.preventDefault();

            const primerError = loginForm.querySelector('.is-invalid');
            if (primerError) {
                primerError.focus();
                primerError.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        }
    });

    contrasenna.addEventListener('input', function () {
        if (this.classList.contains('is-invalid')) {
            removerError(this);
        }
    });
});