const API_BASE = "https://localhost:7095/api/CuentaCorrientes";
let clienteActualId = null;
let clienteActualNombre = "";

async function obtenerSaldoYMovimientos() {
    const nombre = document.getElementById('buscar-cliente').value;
    if (!nombre) return alert("Por favor, ingrese un nombre.");

    try {
        const resp = await fetch(`${API_BASE}/saldo/${nombre}`);
        if (!resp.ok) throw new Error("No se encontró el cliente");
        
        const data = await resp.json();
        console.log("Datos de la API:", data);

        const movimientos = data.Movimientos || data.movimientos;
        if (movimientos && movimientos.length > 0) {
            clienteActualId = movimientos[0].clienteId;
            clienteActualNombre = data.cliente;
        }

        document.getElementById('resumen-saldos').style.display = 'flex';
        document.getElementById('total-deuda').innerText = `$${data.totalDeudaOriginal.toLocaleString('es-AR')}`;
        document.getElementById('total-pagado').innerText = `$${data.totalPagadoHastaHoy.toLocaleString('es-AR')}`;
        document.getElementById('saldo-pendiente').innerText = `$${data.saldoPendiente.toLocaleString('es-AR')}`;
        document.getElementById('nombre-cliente-titulo').innerText = data.cliente;

        dibujarTabla(movimientos);

    } catch (err) {
        alert(err.message);
        console.error("Error en búsqueda:", err);
    }
}

function dibujarTabla(datos) {
    const tabla = document.getElementById('tabla-movimientos');
    tabla.innerHTML = ""; 

    if (!datos || datos.length === 0) {
        tabla.innerHTML = '<tr><td colspan="5" class="text-center text-muted">No hay movimientos registrados.</td></tr>';
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
                <td class="fw-bold">$${saldo.toLocaleString('es-AR')}</td>
            </tr>
        `;
    });
    tabla.innerHTML = html;
}

function prepararPagoGeneral() {
    if (!clienteActualId) return alert("Primero buscá un cliente.");
    registrarPago(clienteActualId, clienteActualNombre);
}

async function registrarPago(clienteId, nombre) {
    const monto = prompt(`¿Cuánto entregó ${nombre}?`);
    if (!monto || isNaN(monto)) return;

    const detalle = prompt("Referencia del pago:", "Efectivo");
    if (detalle === null) return;

    try {
        const url = `${API_BASE}/registrar-pago?clienteId=${clienteId}&monto=${monto}&detalleDelPago=${encodeURIComponent(detalle)}`;
        const response = await fetch(url, { method: 'POST' });

        if (response.ok) {
            const resJSON = await response.json();
            alert(`✅ Pago registrado. Nuevo saldo: $${resJSON.saldoRestante}`);
            obtenerSaldoYMovimientos();
        } else {
            alert("❌ Error: " + await response.text());
        }
    } catch (error) {
        alert("Fallo la conexión con el servidor.");
    }
}