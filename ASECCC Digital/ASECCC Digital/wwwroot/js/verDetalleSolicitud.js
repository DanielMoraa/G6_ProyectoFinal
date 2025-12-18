function verDetalleSolicitud(id) {

    fetch(`/Prestamos/ObtenerSolicitudDetalleAjax?solicitudId=${id}`)
        .then(r => r.json())
        .then(d => {
            const estadoBadge = document.getElementById('estadoSolicitud');
            estadoBadge.innerText = d.estadoSolicitud;

            estadoBadge.classList.remove(
                'bg-success',
                'bg-warning',
                'bg-danger',
                'bg-secondary'
            );

            switch (d.estadoSolicitud.toLowerCase()) {
                case 'aprobada':
                    estadoBadge.classList.add('bg-success');
                    break;

                case 'en revisión':
                case 'en revision':
                    estadoBadge.classList.add('bg-warning', 'text-dark');
                    break;

                case 'rechazado':
                    estadoBadge.classList.add('bg-danger');
                    break;

                default:
                    estadoBadge.classList.add('bg-secondary');
                    break;
            }

            document.getElementById('solicitud-id').value = d.solicitudPrestamoId;
            document.getElementById('usuario-id').value = d.usuarioId;

            document.getElementById('estadoCivil').innerText = d.estadoCivil;
            document.getElementById('pagaAlquiler').innerText = d.pagaAlquiler === 1 ? 'Sí' : 'No';

            if (d.montoAlquiler !== null) {
                document.getElementById('montoAlquilerRow').style.display = 'block';
                document.getElementById('montoAlquiler').innerText = d.montoAlquiler.toFixed(2);
            } else {
                document.getElementById('montoAlquilerRow').style.display = 'none';
            }

            document.getElementById('tipoPrestamo').innerText = d.tipoPrestamo;
            document.getElementById('montoSolicitud').innerText = d.montoSolicitud.toFixed(2);
            document.getElementById('cuotaSemanal').innerText = d.cuotaSemanalSolicitud.toFixed(2);
            document.getElementById('plazoMeses').innerText = d.plazoMeses;
            document.getElementById('estadoSolicitud').innerText = d.estadoSolicitud;
            document.getElementById('propositoPrestamo').innerText = d.propositoPrestamo;

            if (d.nombreAcreedor !== null) {
                document.getElementById('deudasSection').style.display = 'block';
                document.getElementById('nombreAcreedor').innerText = d.nombreAcreedor;
                document.getElementById('totalCredito').innerText = d.totalCredito?.toFixed(2);
                document.getElementById('saldoCredito').innerText = d.saldoCredito?.toFixed(2);
                document.getElementById('abonoSemanal').innerText = d.abonoSemanal?.toFixed(2);
            } else {
                document.getElementById('deudasSection').style.display = 'none';
            }

            if (d.nombreDeudor !== null) {
                document.getElementById('fiadorSection').style.display = 'block';
                document.getElementById('nombreDeudor').innerText = d.nombreDeudor;
                document.getElementById('totalPrestamo').innerText = d.totalPrestamo?.toFixed(2);
                document.getElementById('saldoPrestamo').innerText = d.saldoPrestamo?.toFixed(2);
            } else {
                document.getElementById('fiadorSection').style.display = 'none';
            }




            new bootstrap.Modal('#solicitudDetallesModal').show();
        });
}
