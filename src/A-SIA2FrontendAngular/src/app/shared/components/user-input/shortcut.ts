import { StringMap } from '@angular/compiler/src/compiler_facade_interface';

/**
 * @param {boolean} allowedInInputFields if set to true, the shortcut will not be executed when the user has a input, select or textarea selected.
 * @param {boolean} preventDefault if set to true, the native implementation of the shortcut will be prevented (e.g. browser zoom on ctrl +).
 * @param {boolean | undefined} ctrl whether the ctrl modifier is necessary. Will automatically convert to CMD on macos.
 * @param {boolean | undefined} shift whether the shift modifier is necessary.
 * @param {boolean | undefined} alt whether the alt modifier is necessary.
 * @param {string} key the key to be pressed, should be lowercase.
 * For more information as to what the key should be
 * https://medium.com/claritydesignsystem/angular-pseudo-events-d4e7f89247ee
 */
export type ShortcutConfig = {
  allowedInInputFields?: boolean;
  preventDefault?: boolean;
  ctrl?: boolean;
  shift?: boolean;
  alt?: boolean;
  key: string;
  description: string;
};

/**
 * Contains the shortcut config {@link ShortcutConfig} and helper functions
 * for getting an event string / a visual representation of the shortcut.
 */
export class Shortcut {
  /**
   * Because mac users usually use their command key (CMD), which is a meta key.
   * Windows users usually use their control key (CTRL), which is a control key.
   * WIN == CMD, Control == CTRL
   * This bool is used to convert between ctrl and cmd depending on the users os.
   */
  // https://stackoverflow.com/questions/29033081/setting-os-specific-keybindings-cmd-on-mac-and-ctrl-on-everything-else
  private static isMac = /mac/i.test(
    // reason for disable: ts doesn't have definition for userAgentData.
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (navigator as any).userAgentData ? (navigator as any).userAgentData.platform : navigator.platform
  );

  private settings: ShortcutConfig = {
    allowedInInputFields: false,
    preventDefault: false,
    ctrl: false,
    shift: false,
    alt: false,
    key: '',
    description: 'description is missing'
  };

  /**
   * assigns parameters to settings if they were defined
   */
  constructor(shortcutConfig: ShortcutConfig) {
    this.settings = shortcutConfig;
    let key: keyof ShortcutConfig;
    for (key in shortcutConfig) {
      if (Object.prototype.hasOwnProperty.call(shortcutConfig, key))
        if (typeof shortcutConfig[key] !== undefined)
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          (this.settings[key] as any) = shortcutConfig[key];
    }
  }

  /**
   * Builds the string to be used by renderer2.listen().
   *
   * Will be in format "keydown.control.shift.alt.KEY",
   * or "keydown.meta.shift.alt.KEY" on macos.
   */
  get EventString(): string {
    let eventString = 'keydown.';
    if (this.settings.ctrl)
      if (Shortcut.isMac) eventString += 'meta.';
      else eventString += 'control.';
    if (this.settings.shift) eventString += 'shift.';
    if (this.settings.alt) eventString += 'alt.';
    return (eventString += this.settings.key);
  }

  /**
   * Creates string that is visually representative for this shortcut (to the end user).
   */
  get HTMLString(): string {
    let htmlString = '';
    if (this.settings.ctrl)
      if (Shortcut.isMac) htmlString += 'CMD ';
      else htmlString += 'CTRL ';
    if (this.settings.shift) htmlString += 'SHIFT ';
    if (this.settings.alt) htmlString += 'ALT ';
    return (htmlString += this.settings.key.toUpperCase());
  }

  get Description(): string {
    return this.settings.description;
  }

  get AllowedInInputFields(): boolean {
    return this.settings.allowedInInputFields as boolean;
  }

  get PreventDefault(): boolean {
    return this.settings.preventDefault as boolean;
  }
}
