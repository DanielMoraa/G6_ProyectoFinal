function buscarAsociado() {
    const buscarNombre = document.getElementById("buscarNombre").value.trim();

    if (!buscarNombre) {
        Swal.fire("Atención", "Debe escribir un nombre para buscar.", "warning");
        return;
    }

    fetch("/Usuario/BuscarAsociado", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ NombreCompleto: buscarNombre })
    })
        .then(r => r.json())
        .then(response => {

            if (!response.success) {
                document.getElementById("resultadosBusqueda").style.display = "none";
                document.getElementById("sinResultados").style.display = "block";
                return;
            }

            document.getElementById("resultadosBusqueda").style.display = "block";
            document.getElementById("sinResultados").style.display = "none";

            const tbody = document.getElementById("tablaResultados");
            tbody.innerHTML = "";

            response.data.forEach(item => {
                tbody.innerHTML += `
                <tr>
                    <td>${item.nombreCompleto}</td>
                    <td>${item.identificacion}</td>
                    <td>${item.correoElectronico}</td>
                    <td>${item.telefono}</td>
                    <td>
                        <button class="btn btn-danger btn-sm"
                                onclick="desactivarAsociado(${item.usuarioId})">
                            Desactivar
                        </button>
                    </td>
                </tr>
            `;
            });
        })
        .catch(error => {
            console.error("Error:", error);
            Swal.fire("Error", "Ocurrió un error al buscar el asociado", "error");
        });
}

function desactivarAsociado(usuarioId) {

    Swal.fire({
        title: "¿Está seguro?",
        text: "El asociado será desactivado",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Sí, desactivar",
        cancelButtonText: "Cancelar"
    }).then(result => {

        if (!result.isConfirmed)
            return;

        fetch("/Usuario/DesactivarAsociado", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ UsuarioId: usuarioId })
        })
            .then(r => r.json())
            .then(response => {

                if (!response.success) {
                    Swal.fire("Error", response.message, "error");
                    return;
                }

                Swal.fire("Correcto", response.message, "success");

                // Opcional: refrescar resultados
                buscarAsociado();
            })
            .catch(error => {
                console.error(error);
                Swal.fire("Error", "No se pudo desactivar el asociado", "error");
            });
    });
}

