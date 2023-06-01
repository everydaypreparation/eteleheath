import { EMROTemplatePage } from './app.po';

describe('EMRO App', function() {
  let page: EMROTemplatePage;

  beforeEach(() => {
    page = new EMROTemplatePage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
