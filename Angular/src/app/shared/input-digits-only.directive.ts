import { Directive, HostListener } from '@angular/core';

@Directive({
  selector: '[inputDigitsOnly]',
})
export class InputDigitsOnlyDirective {
  private static readonly allowedKeyCodes = [
    "Backspace",
    "Delete",
    "Insert",
    "ArrowUp",
    "ArrowRight",
    "ArrowDown",
    "ArrowLeft",
    "Tab",
    "Home",
    "End",
    "Enter",
    "Digit1",
    "Digit2",
    "Digit3",
    "Digit4",
    "Digit5",
    "Digit6",
    "Digit7",
    "Digit8",
    "Digit9",
    "Digit0",
    "Numpad0",
    "Numpad1",
    "Numpad2",
    "Numpad3",
    "Numpad4",
    "Numpad5",
    "Numpad6",
    "Numpad7",
    "Numpad8",
    "Numpad9",
  ];

  @HostListener('keydown', ['$event'])
  onKeyDown(e: KeyboardEvent) {
    // This condition checks whether a keyboard control key was pressed.
    // I've left this 'open' on purpose, because I don't want to restrict which commands
    // can be executed in the browser. For example, It wouldn't make sense for me to prevent
    // a find command (Ctrl + F or Command + F) just for having the focus on the input with
    // this directive.
    const isCommandExecution = e.ctrlKey || e.metaKey || e.altKey;
    const isKeyAllowed = InputDigitsOnlyDirective.allowedKeyCodes.indexOf(e.code) !== -1;

    if ((!isCommandExecution && !isKeyAllowed) || e.shiftKey) {
      e.preventDefault();
      return;  // let it happen, don't do anything
    }
  }
}