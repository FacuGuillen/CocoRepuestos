const API_BASE = "https://localhost:7095/api/CuentaCorrientes";
let clienteActualId = null;
let clienteActualNombre = "";

async function obtenerSaldoYMovimientos() {
    const nombreInput = document.getElementById('buscar-cliente').value.trim();
    if (!nombreInput) return alert("Por favor, ingrese un nombre.");

    try {
        const resp = await fetch(`${API_BASE}/saldo/${nombreInput}`);
        if (!resp.ok) throw new Error("No se encontró el cliente o hubo un error en el servidor.");
        
        const data = await resp.json();

        document.getElementById('resumen-saldos').style.display = 'flex';
        document.getElementById('total-deuda').innerText = `$${(data.totalDeudaOriginal || 0).toLocaleString('es-AR')}`;
        document.getElementById('total-pagado').innerText = `$${(data.totalPagadoHastaHoy || 0).toLocaleString('es-AR')}`;
        document.getElementById('saldo-pendiente').innerText = `$${(data.saldoPendiente || 0).toLocaleString('es-AR')}`;
        document.getElementById('nombre-cliente-titulo').innerText = data.cliente || "Cliente Desconocido";

        const movimientos = data.movimientos || data.Movimientos || [];
        
        if (data.clienteId) {
            clienteActualId = data.clienteId;
        } else if (movimientos.length > 0) {
            clienteActualId = movimientos[0].clienteId || movimientos[0].ClienteId;
        } else {
            clienteActualId = null; 
        }

        clienteActualNombre = data.cliente;

        dibujarTabla(movimientos);

    } catch (err) {
        alert(err.message);
        console.error("Error en búsqueda:", err);
        clienteActualId = null;
        document.getElementById('resumen-saldos').style.display = 'none';
    }
}

function dibujarTabla(datos) {
    const tabla = document.getElementById('tabla-movimientos');
    if (!datos || datos.length === 0) {
        tabla.innerHTML = '<tr><td colspan="5" class="text-center text-muted py-4">No hay movimientos registrados.</td></tr>';
        return;
    }

    let html = "";
    datos.forEach(mov => {
        const fecha = mov.fecha || mov.Fecha || "-";
        const detalle = mov.detalle || mov.Detalle || "Venta/Pago";
        const debe = mov.debe !== undefined ? mov.debe : (mov.Debe || 0);
        const haber = mov.haber !== undefined ? mov.haber : (mov.Haber || 0);
        const saldo = mov.saldoAcumulado || mov.SaldoAcumulado || 0;

        html += `
            <tr>
                <td>${new Date(fecha).toLocaleDateString('es-AR')}</td>
                <td>${detalle}</td>
                <td class="text-danger">${debe > 0 ? '$' + debe.toLocaleString('es-AR') : '-'}</td>
                <td class="text-success">${haber > 0 ? '$' + haber.toLocaleString('es-AR') : '-'}</td>
                <td class="fw-bold">$${saldo.toLocaleString('es-AR', { minimumFractionDigits: 2 })}</td>
            </tr>
        `;
    });
    tabla.innerHTML = html;
}

function abrirModalCliente() {
    new bootstrap.Modal(document.getElementById('modalCliente')).show();
}

async function guardarCliente() {
    const nombre = document.getElementById('c_nombre').value.trim();
    const telefono = document.getElementById('c_telefono').value.trim();
    const email = document.getElementById('c_email').value.trim();

    if (!nombre) return alert("El nombre es obligatorio.");

    try {
        const response = await fetch(`https://localhost:7095/api/Clientes`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ nombre, telefono, email })
        });

        if (response.ok) {
            alert("✅ Cliente registrado.");
            location.reload();
        } else {
            alert("Error al guardar: " + await response.text());
        }
    } catch (error) {
        alert("Error de conexión al guardar cliente.");
    }
}

function abrirModalPago() {
    if (!clienteActualId) return alert("Primero buscá un cliente y asegurate de que tenga un ID válido.");
    
    document.getElementById('p_fecha').value = new Date().toISOString().split('T')[0];
    const modalElement = document.getElementById('modalPago');
    const modal = new bootstrap.Modal(modalElement);
    modal.show();
}

async function guardarPago() {
    const monto = document.getElementById('p_monto').value;
    const detalle = document.getElementById('p_detalle').value || "Efectivo";

    if (!monto || isNaN(monto) || monto <= 0) return alert("Ingresá un monto válido mayor a 0.");
    if (!clienteActualId) return alert("Error de sesión: No hay un ID de cliente seleccionado.");

    try {
        const url = `${API_BASE}/registrar-pago?clienteId=${clienteActualId}&monto=${monto}&detalleDelPago=${encodeURIComponent(detalle)}`;
        
        const response = await fetch(url, { method: 'POST' });

        if (response.ok) {
            const resJSON = await response.json();
            alert(`✅ Pago registrado.\nNuevo saldo: $${resJSON.saldoRestante.toLocaleString('es-AR')}`);

            const modalPago = bootstrap.Modal.getInstance(document.getElementById('modalPago'));
            modalPago.hide();

            document.getElementById('p_monto').value = "";
            document.getElementById('p_detalle').value = "";

            obtenerSaldoYMovimientos(); 
        } else {
            const errorText = await response.text();
            alert("❌ Error del servidor: " + errorText);
        }
    } catch (error) {
        console.error("Error en guardarPago:", error);
        alert("Falló la conexión con el servidor al intentar registrar el pago.");
    }
}