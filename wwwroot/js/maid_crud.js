const deleteMaidModalButtons = document.querySelectorAll(".open-delete-maid-modal")

deleteMaidModalButtons.forEach(btn => btn.addEventListener("click", (e) => {
    const maidId = e.currentTarget.dataset.id
    const deleteForm = document.getElementById("deleteMaidRecordForm")
    document.querySelector(".delete-maid-modal-text-content").textContent = `Delete maid with ID ${maidId}? Deleted records cannot be recovered.`
    deleteForm.setAttribute("action", `/Maid/DeleteMaidRecord?maidId=${maidId}`)
}))