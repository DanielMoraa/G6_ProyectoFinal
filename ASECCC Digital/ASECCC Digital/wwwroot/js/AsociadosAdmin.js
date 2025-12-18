$(document).ready(function () {

    $('#tablaAsociados').DataTable({
        processing: true,
        serverSide: true,
        responsive: true,
        autoWidth: false,
        pageLength: 10,
        lengthChange: true,
        language: {
            url: "//cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json"
        },
        ajax: {
            url: '/Usuario/ListarAsociados',
            type: 'POST'
        },
        columns: [
            { data: 'NombreCompleto' },
            { data: 'Identificacion' },
            { data: 'CorreoElectronico' },
            { data: 'Telefono' },
            {
                data: 'UsuarioId',
                orderable: false,
                className: 'text-center',
                render: function (data) {
                    return `
                        <button class="btn btn-danger btn-sm"
                                onclick="desactivar(${data})"
                                title="Desactivar">
                            <i class="ti ti-user-x"></i>
                        </button>

                        <button class="btn btn-warning btn-sm"
                                onclick="abrirModalLiquidacion(${data})">
                            Liquidar
                        </button>
                    `;
                }
            }
        ]
    });

});

function desactivar(id) {
    Swal.fire({
        title: '¿Desactivar asociado?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí',
        cancelButtonText: 'Cancelar'
    }).then(result => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Usuario/DesactivarAsociado',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ UsuarioId: id }),
                success: function (response) {
                    if (response.success) {
                        Swal.fire('Éxito', response.message, 'success');
                        $('#tablaAsociados').DataTable().ajax.reload();
                    } else {
                        Swal.fire('Error', response.message, 'error');
                    }
                },
                error: function () {
                    Swal.fire('Error', 'No se pudo desactivar el asociado', 'error');
                }
            });
        }
    });
}

function abrirModalLiquidacion(usuarioId) {
    fetch('/Usuario/ObtenerRubrosLiquidacion', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ UsuarioId: usuarioId })
    })
        .then(r => r.json())
        .then(data => {
            if (!data.success) {
                Swal.fire("Atención", data.message || "No se pudo obtener los rubros", "warning");
                return;
            }

            document.getElementById("nombreAsociadoModal").textContent = data.nombreCompleto;

            const tbody = document.getElementById("tablaRubrosModal");
            tbody.innerHTML = "";

            if (!data.rubros || data.rubros.length === 0) {
                document.getElementById("sinRubrosModal").classList.remove("d-none");
            } else {
                document.getElementById("sinRubrosModal").classList.add("d-none");

                data.rubros.forEach(r => {
                    tbody.innerHTML += `
                        <tr>
                            <td>${r.tipo}</td>
                            <td>${r.descripcion}</td>
                            <td>₡${r.saldo.toLocaleString('es-CR', { minimumFractionDigits: 2 })}</td>
                            <td>
                                <button class="btn btn-success btn-sm"
                                    onclick="liquidarRubro(${data.usuarioId}, '${r.tipo}', ${r.idRubro})">
                                    Liquidar
                                </button>
                            </td>
                        </tr>
                    `;
                });
            }

            new bootstrap.Modal('#modalLiquidacion').show();
        })
        .catch(err => {
            console.error(err);
            Swal.fire("Error", "Error al obtener los rubros", "error");
        });
}

function liquidarRubro(usuarioId, tipo, idRubro) {
    Swal.fire({
        title: "¿Está seguro?",
        text: `Se liquidará el ${tipo.toLowerCase()} seleccionado`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Sí, liquidar",
        cancelButtonText: "Cancelar"
    }).then(result => {
        if (!result.isConfirmed) return;

        fetch("/Usuario/LiquidarRubro", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                UsuarioId: usuarioId,
                TipoRubro: tipo,
                IdRubro: idRubro
            })
        })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    Swal.fire("Éxito", "Rubro liquidado correctamente", "success");

                    // ✅ Solo recarga los rubros sin cerrar/abrir el modal
                    recargarRubrosModal(usuarioId);
                } else {
                    Swal.fire("Error", data.message || "No se pudo liquidar", "error");
                }
            })
            .catch(err => {
                console.error(err);
                Swal.fire("Error", "Error al procesar la liquidación", "error");
            });
    });
}

function recargarRubrosModal(usuarioId) {
    fetch('/Usuario/ObtenerRubrosLiquidacion', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ UsuarioId: usuarioId })
    })
        .then(r => r.json())
        .then(data => {
            if (!data.success) {
                Swal.fire("Atención", data.message || "No se pudo obtener los rubros", "warning");
                return;
            }

            const tbody = document.getElementById("tablaRubrosModal");
            tbody.innerHTML = "";

            if (!data.rubros || data.rubros.length === 0) {
                document.getElementById("sinRubrosModal").classList.remove("d-none");
                tbody.parentElement.parentElement.classList.add("d-none"); // Ocultar tabla
            } else {
                document.getElementById("sinRubrosModal").classList.add("d-none");
                tbody.parentElement.parentElement.classList.remove("d-none"); // Mostrar tabla

                data.rubros.forEach(r => {
                    tbody.innerHTML += `
                        <tr>
                            <td>${r.tipo}</td>
                            <td>${r.descripcion}</td>
                            <td>₡${r.saldo.toLocaleString('es-CR', { minimumFractionDigits: 2 })}</td>
                            <td>
                                <button class="btn btn-success btn-sm"
                                    onclick="liquidarRubro(${data.usuarioId}, '${r.tipo}', ${r.idRubro})">
                                    Liquidar
                                </button>
                            </td>
                        </tr>
                    `;
                });
            }
        })
        .catch(err => {
            console.error(err);
            Swal.fire("Error", "Error al recargar los rubros", "error");
        });
}
