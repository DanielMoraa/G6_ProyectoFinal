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
        .then(data => {
            if (data.success) {
                document.getElementById("resultadosBusqueda").style.display = "block";
                document.getElementById("sinResultados").style.display = "none";
                const t = document.getElementById("tablaResultados");
                t.innerHTML = `
                <tr>
                    <td>${data.nombre}</td>
                    <td>${data.identificacion}</td>
                    <td>${data.correo}</td>
                    <td>${data.telefono}</td>
                    <td>
                        <button class="btn btn-danger btn-sm"
                                onclick="desactivarAsociado(${data.id})">
                            Desactivar
                        </button>
                    </td>
                </tr>`;
            } else {
                document.getElementById("resultadosBusqueda").style.display = "none";
                document.getElementById("sinResultados").style.display = "block";
            }
        })
        .catch(error => {
            console.error("Error:", error);
            Swal.fire("Error", "Ocurrió un error al buscar el asociado", "error");
        });
}

function desactivarAsociado(usuarioId) {
    Swal.fire({
        title: "¿Está seguro?",
        text: "El asociado será desactivado.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#d33",
        cancelButtonColor: "#3085d6",
        confirmButtonText: "Sí, desactivar",
        cancelButtonText: "Cancelar"
    }).then(result => {
        if (!result.isConfirmed) return;

        fetch("/Usuario/DesactivarAsociado", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ UsuarioId: usuarioId })
        })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    Swal.fire("Éxito", "Asociado desactivado correctamente", "success");
                    document.getElementById("resultadosBusqueda").style.display = "none";
                    document.getElementById("buscarNombre").value = "";
                } else {
                    Swal.fire("Error", data.message || "No se pudo desactivar el asociado", "error");
                }
            })
            .catch(error => {
                console.error("Error:", error);
                Swal.fire("Error", "Ocurrió un error al desactivar el asociado", "error");
            });
    });
}