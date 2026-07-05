import { Component, Optional, Self, input, signal } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';

@Component({
  selector: 'app-form-input',
  standalone: true,
  templateUrl: './form-input.html',
  styleUrl: './form-input.scss'
})
export class FormInput implements ControlValueAccessor {
  label = input.required<string>();
  type = input('text');
  autocomplete = input('');
  placeholder = input('');
  errorText = input('שדה חובה');

  value = signal('');
  disabled = signal(false);

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  constructor(@Self() @Optional() public ngControl: NgControl) {
    if (this.ngControl) {
      this.ngControl.valueAccessor = this;
    }
  }

  writeValue(value: string | null): void {
    this.value.set(value ?? '');
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  onInput(value: string): void {
    this.value.set(value);
    this.onChange(value);
  }

  onBlur(): void {
    this.onTouched();
  }

  showError(): boolean {
    const control = this.ngControl?.control;

    return !!control && control.touched && control.invalid;
  }
}