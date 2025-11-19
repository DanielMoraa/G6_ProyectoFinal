let usuarioIdActual = 0;

function buscarAsociadoLiquidacion() {
    const buscarNombre = document.getElementById("buscarAsociado").value.trim();

    if (!buscarNombre) {
        Swal.fire("Atención", "Debe escribir un nombre para buscar.", "warning");
        return;
    }

    fetch("/Usuario/ObtenerRubrosLiquidacion", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ NombreCompleto: buscarNombre })
    })
        .then(r => r.json())
        .then(data => {
            if (data.success && data.rubros && data.rubros.length > 0) {
                usuarioIdActual = data.usuarioId;
                document.getElementById("nombreAsociado").textContent = data.nombreCompleto;
                document.getElementById("detalleCuenta").style.display = "block";
                document.getElementById("sinRubros").style.display = "none";

                const tbody = document.getElementById("tablaRubros");
                tbody.innerHTML = "";

                data.rubros.forEach(rubro => {
                    const tr = document.createElement("tr");
                    tr.innerHTML = `
                        <td>${rubro.tipo}</td>
                        <td>${rubro.descripcion}</td>
                        <td>₡${rubro.saldo.toLocaleString('es-CR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
                        <td>
                            <button class="btn btn-success btn-sm" 
                                    onclick="liquidarRubro('${rubro.tipo}', '${rubro.descripcion}', ${rubro.saldo}, ${rubro.idRubro})">
                                Liquidar
                            </button>
                        </td>
                    `;
                    tbody.appendChild(tr);
                });
            } else {
                document.getElementById("detalleCuenta").style.display = "none";
                document.getElementById("sinRubros").style.display = "block";
                usuarioIdActual = 0;
            }
        })
        .catch(error => {
            console.error("Error:", error);
            Swal.fire("Error", "Ocurrió un error al buscar el asociado", "error");
        });
}

function liquidarRubro(tipo, descripcion, saldo, idRubro) {
    Swal.fire({
        title: "¿Está seguro?",
        html: `El rubro <strong>"${descripcion}"</strong> con saldo de <strong>₡${saldo.toLocaleString('es-CR', { minimumFractionDigits: 2 })}</strong> será liquidado.`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#28a745",
        cancelButtonColor: "#6c757d",
        confirmButtonText: "Sí, liquidar",
        cancelButtonText: "Cancelar"
    }).then(result => {
        if (!result.isConfirmed) return;

        fetch("/Usuario/LiquidarRubro", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                UsuarioId: usuarioIdActual,
                TipoRubro: tipo,
                IdRubro: idRubro
            })
        })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    Swal.fire("Éxito", "Rubro liquidado correctamente", "success");
                    buscarAsociadoLiquidacion(); 
                } else {
                    Swal.fire("Error", data.message || "No se pudo liquidar el rubro", "error");
                }
            })
            .catch(error => {
                console.error("Error:", error);
                Swal.fire("Error", "Ocurrió un error al liquidar el rubro", "error");
            });
    });
}