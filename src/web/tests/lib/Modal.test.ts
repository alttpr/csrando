import { fireEvent, render } from "@testing-library/svelte";
import { describe, expect, it, vi } from "vitest";
import { createRawSnippet } from "svelte";
import { tick } from "svelte";
import Modal from "$lib/components/ui/Modal.svelte";

const children = createRawSnippet(() => ({
  render: () =>
    `<div><input aria-label="first field" /><button data-autofocus>Primary action</button></div>`,
}));

function renderModal(props: Record<string, unknown> = {}) {
  return render(Modal, {
    props: {
      open: true,
      title: "Test dialog",
      children,
      ...props,
    },
  });
}

describe("Modal", () => {
  it("renders an accessible dialog labelled by its title", () => {
    const { getByRole } = renderModal();
    const dialog = getByRole("dialog");
    expect(dialog.getAttribute("aria-modal")).toBe("true");
    const labelId = dialog.getAttribute("aria-labelledby");
    expect(labelId).toBeTruthy();
    expect(document.getElementById(labelId!)?.textContent).toContain(
      "Test dialog",
    );
  });

  it("moves focus to the data-autofocus element when opened", async () => {
    const { getByText } = renderModal();
    await tick();
    expect(document.activeElement).toBe(getByText("Primary action"));
  });

  it("closes on Escape and reports through onclose", async () => {
    const onclose = vi.fn();
    const { queryByRole } = renderModal({ onclose });
    await tick();
    await fireEvent.keyDown(window, { key: "Escape" });
    expect(onclose).toHaveBeenCalledTimes(1);
    expect(queryByRole("dialog")).toBeNull();
  });

  it("traps Tab focus inside the dialog", async () => {
    const { getByRole, getByText, getByLabelText } = renderModal();
    await tick();
    const dialog = getByRole("dialog");
    const primary = getByText("Primary action");
    const input = getByLabelText("first field");

    // Tab from the last focusable wraps to the first.
    const focusables = dialog.querySelectorAll<HTMLElement>("button, input");
    const last = focusables[focusables.length - 1];
    last.focus();
    await fireEvent.keyDown(window, { key: "Tab" });
    expect(dialog.contains(document.activeElement)).toBe(true);
    expect(document.activeElement).not.toBe(last);

    // Shift+Tab from the first wraps to the last.
    const first = focusables[0];
    first.focus();
    await fireEvent.keyDown(window, { key: "Tab", shiftKey: true });
    expect(dialog.contains(document.activeElement)).toBe(true);
    expect(document.activeElement).toBe(last);

    void primary;
    void input;
  });

  it("restores focus to the previously focused element on close", async () => {
    const outside = document.createElement("button");
    outside.textContent = "outside";
    document.body.appendChild(outside);
    outside.focus();

    const { rerender } = renderModal({ open: false });
    await rerender({ open: true });
    await tick();
    expect(document.activeElement).not.toBe(outside);

    await rerender({ open: false });
    await tick();
    expect(document.activeElement).toBe(outside);
    outside.remove();
  });

  it("closes on backdrop click when enabled", async () => {
    const onclose = vi.fn();
    const { getByTestId, queryByRole } = renderModal({ onclose });
    await fireEvent.click(getByTestId("modal-backdrop"));
    expect(onclose).toHaveBeenCalled();
    expect(queryByRole("dialog")).toBeNull();
  });
});
